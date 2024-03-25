namespace FuelWise.OBDProtocol;

public record Payload(Mode Mode, PID PID, Data Data)
{
    public FormattedByte NumberOfBytes => CalculateSize();

    private FormattedByte CalculateSize()
    {
        return Data.Size + 0x01 + 0x01;
    }

    public override string ToString()
    {
        return $"{NumberOfBytes}{Mode.ToHex()}{PID.ToHex()}{Data}";
    }
}