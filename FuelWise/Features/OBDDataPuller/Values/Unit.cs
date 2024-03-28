namespace FuelWise.OBDDataPuller;

public readonly record struct Unit(string Name, string Symbol)
{
    public static Unit None => new("None", string.Empty);

    public static Unit Percent => new("Percent", "%");

    public static Unit Celsius => new("Celsius", "°C");

    public static Unit Kpa => new("Kilopascal", "kPa");

    public static Unit Rpm => new("Revolutions per minute", "RPM");

    public static Unit KilometerPerHour => new("Kilometer per hour", "km/h");

    public static Unit Degrees => new("Degrees", "°");

    public static Unit GramsPerSecond => new("Grams per second", "g/s");

    public static Unit Seconds => new("Seconds", "s");

    public static Unit Kilometer => new("Kilometer", "km");

    public static Unit Liter => new("Liter", "L");

    public static Unit LitersPerHour => new("Liters per hour", "L/h");

    public static Unit Ratio => new("Ratio", "ratio");

    public string ToFormattedString(object value)
    {
        return $"{value}{Symbol}";
    }

    public string ToFormattedLong(object value)
    {
        return $"{value} {Name.ToLower()}";
    }

    public override string ToString()
    {
        return Symbol;
    }
}