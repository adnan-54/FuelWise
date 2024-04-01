using FuelWise.VehicleInformations;

namespace FuelWise.WiseCalculations;

public interface IWiseCalculations
{
    int GetCurrentGear(double rpm, int speed);

    double GetVolumetricEfficiency(double rpm, double maf);

    double GetCalculatedImap(double rpm, double map, double intakeAirTemperature);

    double GetCalculatedMaf(double imap, double volumetricEfficiency);
}

internal class DefaultWiseCalculations : IWiseCalculations
{
    private readonly IVehicleProvider vehicleProvider;

    public DefaultWiseCalculations(IVehicleProvider vehicleProvider)
    {
        this.vehicleProvider = vehicleProvider;
    }

    public int GetCurrentGear(double rpm, int speed)
    {
        if (rpm == 0 || speed == 0)
            return 0;

        var vehicle = vehicleProvider.Vehicle;

        if (vehicle is null)
            return 0;

        var tyre = vehicle.Tyre;
        var transmission = vehicle.Transmission;
        var gearRatios = transmission.Ratios;

        var possibleSpeeds = gearRatios.Select(gearRatio => GetSpeed(rpm, gearRatio, transmission.FinalRatio, tyre.GetCircunference()));

        var differences = possibleSpeeds.Select(possibleSpeed => Math.Abs(possibleSpeed - speed)).ToList();
        var lowestDifference = differences.Min();
        var lowestDifferenceIndex = differences.IndexOf(lowestDifference);

        var possibleSpeed = possibleSpeeds.ElementAt(lowestDifferenceIndex);
        var differencePercentage = GetSpeedDifference(speed, possibleSpeed);

        if (differencePercentage > 20)
            return 0;

        return lowestDifferenceIndex + 1;
    }

    private static double GetSpeed(double rpm, double gearRatio, double finalRatio, double tireCircunference)
    {
        var rpmReduction = rpm / gearRatio / finalRatio;
        var wheelSpeed = rpmReduction * tireCircunference / 1000000 * 60;
        return wheelSpeed;
    }

    private static double GetSpeedDifference(double currentSpeed, double calculatedSpeed)
    {
        var difference = Math.Abs(currentSpeed - calculatedSpeed);
        var average = (currentSpeed + calculatedSpeed) / 2;
        return difference / average * 100;
    }

    public double GetVolumetricEfficiency(double rpm, double maf)
    {
        if (rpm == 0 || maf == 0)
            return 0;

        var vehicle = vehicleProvider.Vehicle;

        if (vehicle is null)
            return 0;

        var displacementInLiters = vehicle.Engine.Displacement / 1000.0;

        return maf / (rpm * 1.07 * (displacementInLiters / 120.0)) * 100;
    }

    public double GetCalculatedImap(double rpm, double map, double intakeAirTemperature)
    {
        if (rpm == 0 || map == 0)
            return 0;

        var intakeAirTempInKelvins = intakeAirTemperature + 273.15;
        return rpm * map / (intakeAirTempInKelvins / 2.0);
    }

    public double GetCalculatedMaf(double imap, double volumetricEfficiency)
    {
        var vehicle = vehicleProvider.Vehicle;

        if (vehicle is null)
            return 0;

        if (volumetricEfficiency > 1)
            volumetricEfficiency /= 100;

        if (imap == 0 || volumetricEfficiency == 0)
            return 0;

        var displacementInLiters = vehicle.Engine.Displacement / 1000.0;

        return imap / 60.0 * volumetricEfficiency * displacementInLiters * 3.484484;
    }
}