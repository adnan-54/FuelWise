namespace FuelWise.OBDProtocol;

public record Data(DataFragment A = default, DataFragment B = default, DataFragment C = default, DataFragment D = default)
{
    private readonly DataFragment E = new();

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
        return $"{A}{B}{C}{D}{E}";
    }

    public static Data CreateFrom(IEnumerable<DataFragment> fragments)
    {
        var data = fragments.ToArray();

        if (data.Length < 1)
            return new Data();

        if (data.Length < 2)
            return new Data(data[0]);

        if (data.Length < 3)
            return new Data(data[0], data[1]);

        if (data.Length < 4)
            return new Data(data[0], data[1], data[2]);

        return new Data(data[0], data[1], data[2], data[3]);
    }
}