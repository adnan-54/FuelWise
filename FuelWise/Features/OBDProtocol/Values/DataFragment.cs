namespace FuelWise.OBDProtocol;

public readonly record struct DataFragment(FormattedByte? Byte = null)
{
    public readonly bool IsEmpty => Byte is null;

    public override readonly string ToString()
    {
        return Byte?.ToString() ?? 0xAA.ToString("X2");
    }

    public static implicit operator FormattedByte(DataFragment dataFragment) => dataFragment.Byte ?? 0xAA;

    public static implicit operator DataFragment(FormattedByte formattedByte) => new(formattedByte);
}