using TG2.Models;

namespace TG2.Services;

public interface IGearController
{
    int GetCurrentGear(double rpm, int speed);

    int GetCurrentGear(OBD obd);
}

internal class GearController : IGearController
{
    private readonly IVehicleProvider vehicleProvider;

    private Transmission Transmission => vehicleProvider.Transmission;

    public GearController(IVehicleProvider vehicleProvider)
    {
        this.vehicleProvider = vehicleProvider;
    }

    public int GetCurrentGear(double rpm, int speed)
    {
        return GetGear(rpm, speed);
    }

    public int GetCurrentGear(OBD obd)
    {
        return GetGear(obd.RPM, obd.Speed);
    }

    private int GetGear(double rpm, int speed)
    {
        var gearRatios = Transmission.GearRatios;

        var possibleSpeeds = gearRatios.Select(gearRatio => GetSpeed(rpm, gearRatio));
        var differences = possibleSpeeds.Select(possibleSpeed => Math.Abs(possibleSpeed - speed)).ToList();

        var lowestDifference = differences.Min();
        var lowestDifferenceIndex = differences.IndexOf(lowestDifference);

        return lowestDifferenceIndex + 1;
    }

    private double GetSpeed(double rpm, double gearRatio)
    {
        var rpmReduction = (rpm / gearRatio / Transmission.FinalRatio);
        var wheelSpeed = (rpmReduction * vehicleProvider.Wheel.TireCircunference) / 1000000;
        return wheelSpeed * 60;
    }

}