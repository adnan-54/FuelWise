namespace FuelWise.OBDProtocol;

public readonly record struct FormattedByte(byte Byte = default)
{
    public override string ToString()
    {
        return Byte.ToString("X2");
    }

    public static implicit operator byte(FormattedByte formattedByte) => formattedByte.Byte;

    public static implicit operator FormattedByte(byte @byte) => new(@byte);

    public static implicit operator int(FormattedByte formattedByte) => formattedByte.Byte;

    public static implicit operator FormattedByte(int @int) => new((byte)@int);
}