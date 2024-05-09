namespace FuelWise.Reporting;

public static class Extensions
{
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        double avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }

    public static double CoefficientOfVariance(this IEnumerable<double> values)
    {
        var stddev = values.StandardDeviation();
        var avg = values.Average();

        if (avg == 0)
            return 0;

        return stddev / avg * 100;
    }

    public static double ClampTo(this double value, double minValue, double maxValue)
    {
        if (value < minValue)
            return minValue;
        if (value > maxValue)
            return maxValue;
        return value;
    }
}