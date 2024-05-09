using FuelWise.IA;
using FuelWise.Reporting;
using FuelWise.VehicleInformations;

namespace FuelWise.WiseCalculations;

public interface IWiseCalculations
{
    int GetCurrentGear(double rpm, double speed);

    double GetVolumetricEfficiency(double rpm, double maf, double intakeTemperature, double map);

    double GetImap(double rpm, double map, double intakeAirTemperature);

    double GetCalculatedMaf(double imap, double volumetricEfficiency);

    double GetFuelComsumption(double predictedFuelComsumption, double speed, double maf, double tps, double rpm);

    double GetFuelEfficiency(double fuelComsumption, bool isOnHighway);

    double GetAverageFuelComsumption(IEnumerable<double> comsumptions);

    double GetComsumptionVariance(IEnumerable<double> comsumptions);

    TrafficCondition GetTrafficCondition(double averageSpeed, bool isOnHighway);

    double GetAverageSpeed(IEnumerable<double> speeds);
}

internal class DefaultWiseCalculations : IWiseCalculations
{
    private const double MOLAR_MASS = 0.28705;
    private const double MPG_TO_KML = 2.352;

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

        var engineSize = engine.Displacement / 1000.0;

        //var cubicInches = engineSize / 0.0163871;
        //var airFlowLibras = maf * 0.00220462 * 60;

        var fahrenheitDegrees = intakeTemperature * 9 / 5 + 32;
        var celciusDegrees = (fahrenheitDegrees - 32) * 5 / 9;
        var kelvinDegrees = celciusDegrees + 273.15;

        var volume = (maf * kelvinDegrees * MOLAR_MASS) / map;

        var airVolume = volume * 60;
        var theoreticalAirVolume = engineSize * rpm / 2;
        var estimatedVolumetricEfficiency = (airVolume / theoreticalAirVolume) * 100;
        estimatedVolumetricEfficiency = (estimatedVolumetricEfficiency + 75) / 2;
        //var cylinderAir = maf * 120 / (rpm * engine.Cylinders);
        //var referenceCylinderAir = engineSize * 1.168 / engine.Cylinders;

        //var gross = airFlowLibras * 10;
        //var whp = gross * 0.85;
        //var engineLoad = (cylinderAir / referenceCylinderAir) * 100;

        return estimatedVolumetricEfficiency.ClampTo(0, 100);
    }

    public double GetImap(double rpm, double map, double intakeAirTemperature)
    {
        if (rpm == 0 || map == 0)
            return 0;

        var intakeAirTempInKelvins = intakeAirTemperature + 273.15;
        return rpm * map / intakeAirTempInKelvins / 2.0;
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

    public double GetFuelComsumption(double predictedFuelComsumption, double speed, double maf, double tps, double rpm)
    {
        var engine = vehicleProvider.Vehicle?.Engine;

        if (engine is null || speed == 0)
            return predictedFuelComsumption;

        //todo: should use fuel status to detect if engine is in cutoff mode
        if (tps == 0 && rpm > 1250)
            return 42.5;

        var airFuelRatio = engine.GetAirFuelRatio();
        var gramsOfFuel = maf / airFuelRatio;
        var lbsOfFuel = gramsOfFuel / 453.592;
        var galsOfFuel = lbsOfFuel / 6.701;
        var galsPerHour = galsOfFuel * 3600;
        var milesPerHour = speed / 1.6;
        var milesPerGal = galsPerHour == 0 ? 0 : milesPerHour / galsPerHour;
        var calculatedFuelComsumption = milesPerGal / MPG_TO_KML;

        var fuelComsumption = (predictedFuelComsumption + calculatedFuelComsumption) / 2;

        return fuelComsumption;
    }

    public double GetFuelEfficiency(double fuelComsumption, bool isOnHighway)
    {
        var engine = vehicleProvider.Vehicle?.Engine;

        if (engine is null)
            return 100;

        var urbanConsumption = engine.UrbanConsumption;
        var higwayConsumption = engine.HighwayConsumption;
        var targetComsumption = isOnHighway ? higwayConsumption : urbanConsumption;
        var efficiency = (fuelComsumption / targetComsumption);
        efficiency *= 100;

        return efficiency.ClampTo(0, 100);
    }

    public double GetAverageFuelComsumption(IEnumerable<double> comsumptions)
    {
        comsumptions = comsumptions.Where(c => !double.IsInfinity(c));

        if (!comsumptions.Any())
            return 0;

        return comsumptions.Average();
    }

    public double GetComsumptionVariance(IEnumerable<double> comsumptions)
    {
        comsumptions = comsumptions.Where(c => !double.IsInfinity(c));

        if (!comsumptions.Any())
            return 0;

        return comsumptions.CoefficientOfVariance().ClampTo(0, 100);
    }

    public TrafficCondition GetTrafficCondition(double averageSpeed, bool isOnHighway)
    {
        if (isOnHighway)
            return GetHighwayTrafficCondition(averageSpeed);

        return GetUrbanTrafficCondition(averageSpeed);
    }

    private static TrafficCondition GetHighwayTrafficCondition(double averageSpeed)
    {
        if (averageSpeed >= 80)
            return TrafficCondition.LowCongestion;
        if (averageSpeed <= 70)
            return TrafficCondition.HighCongestion;
        return TrafficCondition.NormalCongestion;
    }

    private static TrafficCondition GetUrbanTrafficCondition(double averageSpeed)
    {
        if (averageSpeed > 40)
            return TrafficCondition.LowCongestion;
        if (averageSpeed < 15)
            return TrafficCondition.HighCongestion;
        return TrafficCondition.NormalCongestion;
    }

    public double GetAverageSpeed(IEnumerable<double> speeds)
    {
        if (!speeds.Any())
            return 0;

        return speeds.Average();
    }
}