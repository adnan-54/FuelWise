using System.Globalization;
using System.Text;
using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDEncoder;

public interface IOBDEncoder
{
    byte[] Encode(Frame command);

    Frame Decode(byte[] data);
}

internal class DefaultOBDEncoder : IOBDEncoder
{
    private static readonly CultureInfo CurrentCulture = Thread.CurrentThread.CurrentCulture;

    public byte[] Encode(Frame frame)
    {
        var data = frame.ToString();
        var buffer = Encoding.ASCII.GetBytes(data);

        return buffer;
    }

    public Frame Decode(byte[] buffer)
    {
        var command = Encoding.ASCII.GetString(buffer);
        var bytes = GetBytes(command);

        var canId = GetCanId(bytes);
        var mode = GetMode(bytes);
        var pid = GetPid(bytes);
        var fragments = CreateFragments(bytes);

        var data = Data.CreateFrom(fragments);
        var payload = new Payload(mode, pid, data);
        var frame = new Frame(canId, payload);

        return frame;
    }

    private static byte[] GetBytes(string command)
    {
        command = $"0{command}";
        var bytes = command.Chunk(2)
            .Select(c => new string(c))
            .Select(ParseByte)
            .ToArray();

        if (bytes.Length != 11)
            throw new Exception($"Frame '{command}' não é valido");

        return bytes;
    }

    private static byte ParseByte(string value)
    {
        if (byte.TryParse(value, NumberStyles.HexNumber, CurrentCulture, out var result))
            return result;
        return default;
    }

    private static CanId GetCanId(byte[] bytes)
    {
        var canId = ParseCanId(bytes);

        if (Enum.IsDefined(typeof(CanId), canId))
            return (CanId)canId;

        throw new Exception($"Valor '{canId:x4}' não é um id valido");
    }

    private static int ParseCanId(byte[] bytes)
    {
        var concatenatedBytes = bytes.Take(2).Select(b => b.ToString("X2")).Aggregate((a, b) => a + b);
        if (int.TryParse(concatenatedBytes, NumberStyles.HexNumber, CurrentCulture, out var result))
            return result;
        return default;
    }

    private static int GetNumberOfBytes(byte[] bytes)
    {
        return bytes[2];
    }

    private static Mode GetMode(byte[] bytes)
    {
        var mode = (int)bytes[3];

        if (Enum.IsDefined(typeof(Mode), mode))
            return (Mode)mode;

        throw new Exception($"Valor '{mode:x2}' não é um modo valido");
    }

    private static PID GetPid(byte[] bytes)
    {
        var pid = (int)bytes[4];

        if (Enum.IsDefined(typeof(PID), pid))
            return (PID)pid;

        throw new Exception($"Valor '{pid:x2}' não é um PID valido");
    }

    private static IEnumerable<DataFragment> CreateFragments(byte[] bytes)
    {
        var numberOfBytes = GetNumberOfBytes(bytes) - 2;

        return bytes[5..].Select((b, i) => CreateFragment(b, numberOfBytes > i));
    }

    private static DataFragment CreateFragment(byte value, bool isUsed)
    {
        if (isUsed)
            return new(value);

        return new();
    }
}