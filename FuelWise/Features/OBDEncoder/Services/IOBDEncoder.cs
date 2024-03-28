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
        var data = frame.ToShortRequestString();
        var buffer = Encoding.ASCII.GetBytes(data);

        return buffer;
    }

    public Frame Decode(byte[] buffer)
    {
        var command = Encoding.ASCII.GetString(buffer);
        if (command.Contains('>'))
        {
            var commands = command.Split('>');

            foreach (var possibleCommand in commands.Reverse())
            {
                if (string.IsNullOrEmpty(possibleCommand))
                    continue;

                command = possibleCommand;
                break;
            }
        }

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

        return CanId.Response;
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
        var b = bytes[3];
        var mode = b > 0x40 ? b - 0x40 : b;

        if (Enum.IsDefined(typeof(Mode), mode))
            return (Mode)mode;

        return Mode.Error;
    }

    private static PID GetPid(byte[] bytes)
    {
        var pid = (int)bytes[4];

        if (Enum.IsDefined(typeof(PID), pid))
            return (PID)pid;

        return PID.Unknown;
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