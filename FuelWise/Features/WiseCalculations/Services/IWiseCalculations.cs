using Android.Health.Connect.DataTypes.Units;
using FuelWise.VehicleInformations;

namespace FuelWise.WiseCalculations;

public interface IWiseCalculations
{
    int GetCurrentGear(double rpm, double speed);

    double GetVolumetricEfficiency(double rpm, double maf, double intakeTemperature, double map);

    double GetCalculatedImap(double rpm, double map, double intakeAirTemperature);

    double GetCalculatedMaf(double imap, double volumetricEfficiency);
}

internal class DefaultWiseCalculations : IWiseCalculations
{
    private const double MOLAR_MASS = 0.28705;

    private readonly IVehicleProvider vehicleProvider;

    public DefaultWiseCalculations(IVehicleProvider vehicleProvider)
    {
        this.vehicleProvider = vehicleProvider;
    }

    public int GetCurrentGear(double rpm, double speed)
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

        //var possibleSpeed = possibleSpeeds.ElementAt(lowestDifferenceIndex);
        //var differencePercentage = GetSpeedDifference(speed, possibleSpeed);

        //if (differencePercentage > 20)
        //    return 0;

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

    public double GetVolumetricEfficiency(double rpm, double maf, double intakeTemperature, double map)
    {
        if (rpm == 0 || maf == 0)
            return 0;

        var vehicle = vehicleProvider.Vehicle;

        if (vehicle is null)
            return 0;

        var engine = vehicle.Engine;

        var engineSize = engine.Displacement / 1000;

        //var cubicInches = engineSize / 0.0163871;
        //var airFlowLibras = maf * 0.00220462 * 60;

        var fahrenheitDegrees = intakeTemperature * 9 / 5 + 32;
        var celciusDegrees = (fahrenheitDegrees - 32) * 5 / 9;
        var kelvinDegrees = celciusDegrees + 273.15;

        var volume = (maf * kelvinDegrees * MOLAR_MASS) / map;

        var airVolume = volume * 60;
        var theoreticalAirVolume = engineSize * rpm / 2;
        var estimatedVolumetricEfficiency = (airVolume / theoreticalAirVolume) * 100;

        //var cylinderAir = maf * 120 / (rpm * engine.Cylinders);
        //var referenceCylinderAir = engineSize * 1.168 / engine.Cylinders;

        //var gross = airFlowLibras * 10;
        //var whp = gross * 0.85;
        //var engineLoad = (cylinderAir / referenceCylinderAir) * 100;

        return estimatedVolumetricEfficiency > 100 ? 100 : estimatedVolumetricEfficiency;
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