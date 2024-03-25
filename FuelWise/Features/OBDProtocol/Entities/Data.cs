namespace FuelWise.OBDProtocol;

public record Data(DataFragment A = default, DataFragment B = default, DataFragment C = default, DataFragment D = default)
{
    public FormattedByte Size => CalculateSize();

    private byte CalculateSize()
    {
        if (!D.IsEmpty)
            return 0x04;

        if (!C.IsEmpty)
            return 0x03;

        if (!B.IsEmpty)
            return 0x02;

        if (!A.IsEmpty)
            return 0x01;

        return 0x0;
    }

    public override string ToString()
    {
        return $"{A}{B}{C}{D}";
    }
}