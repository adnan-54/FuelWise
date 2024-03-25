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
    public byte[] Encode(Frame frame)
    {
        var data = frame.ToString();
        var buffer = Encoding.ASCII.GetBytes(data);

        return buffer;
    }

    public Frame Decode(byte[] buffer)
    {
        var command = Encoding.ASCII.GetString(buffer);
        var identifier = int.Parse(command[..3], NumberStyles.HexNumber);
        var bytes = command[3..]
            .Chunk(2)
            .Select(c => new string(c))
            .Select(s => byte.TryParse(s, NumberStyles.HexNumber, new CultureInfo("pt-br"), out var result) ? result : default)
            .ToArray();

        if (bytes.Length < 7)
            throw new Exception($"Frame '{command}' não valido");

        var canId = (CanId)identifier;
        var numberOfBytes = bytes[0] - 0x01 - 0x01;
        var mode = (Mode)bytes[1];
        var pid = (PID)bytes[2];
        var fragments = bytes[3..].Select((b, i) => numberOfBytes > i ? new DataFragment(b) : new DataFragment()).ToArray();

        var data = new Data(fragments[0], fragments[1], fragments[2], fragments[3]);
        var payload = new Payload(mode, pid, data);
        var frame = new Frame(canId, payload);

        return frame;
    }
}