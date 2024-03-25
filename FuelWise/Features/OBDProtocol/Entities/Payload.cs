namespace FuelWise.OBDProtocol;

public record Payload(Mode Mode, PID PID, Data Data)
{
    public override string ToString()
    {
        return $"{Data.Size}{Mode.ToHex()}{PID.ToHex()}{Data}";
    }
}