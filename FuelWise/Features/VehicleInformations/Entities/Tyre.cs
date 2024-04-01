namespace FuelWise.VehicleInformations;

public record Tyre(int Width, int AspectRatio, int Diameter)
{
    public double GetCircunference()
    {
        return Math.PI * (Diameter * 25.4 + (((Width * 100) * (AspectRatio / 100D)) / 100D) * 2);
    }
}