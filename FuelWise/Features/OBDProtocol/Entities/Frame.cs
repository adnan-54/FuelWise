namespace FuelWise.OBDProtocol;

public record Frame(CanId CanId, Payload Payload)
{
    private const char COMMAND_TERMINATOR = '\r';

    public bool IsRequest => CanId == CanId.Request;

    public bool IsResponse => CanId == CanId.Response;

    public DateTime CreatedAt { get; } = DateTime.Now;

    public override string ToString()
    {
        return $"{CanId.ToHex()}{Payload}{COMMAND_TERMINATOR}";
    }

    public string ToShortRequestString()
    {
        return $"{Payload.Mode.ToHex()}{Payload.PID.ToHex()}{COMMAND_TERMINATOR}";
    }
}