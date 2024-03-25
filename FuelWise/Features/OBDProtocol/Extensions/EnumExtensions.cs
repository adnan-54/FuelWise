namespace FuelWise.OBDProtocol;

public static partial class EnumExtensions
{
    public static string ToHex(this Enum value)
    {
        var asInt = (int)(IConvertible)value;

        return asInt.ToString("X2");
    }
}