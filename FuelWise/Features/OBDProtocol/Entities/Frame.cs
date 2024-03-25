namespace FuelWise.OBDProtocol;

public record Frame(CanId CanId, Payload Payload)
{
    private const char LINE_TERMINATOR = '\r';

    public bool IsRequest => CanId == CanId.Request;

    public bool IsResponse => CanId == CanId.Response;

    public override string ToString()
    {
        return $"{CanId.ToHex()}{Payload}{LINE_TERMINATOR}";
    }
}