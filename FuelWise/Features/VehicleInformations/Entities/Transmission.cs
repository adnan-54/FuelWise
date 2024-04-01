namespace FuelWise.VehicleInformations;

public record Transmission(double[] Ratios, double FinalRatio)
{
    public int GearCount => Ratios.Length;

    public double GetGearRatio(int gear)
    {
        return Ratios[gear - 1];
    }
}