using TG2.Models;

namespace TG2.Services;

public interface IConsumptionCalculator
{
    /// <summary>
    /// Calculates the consumption of the car in liters per 100 km
    /// </summary>
    /// <param name="obd">The last read values from the car</param>
    /// <returns>The consumption in liters</returns>
    double GetCalculatedConsumption(OBD obd);

    /// <summary>
    /// Calculates the remaining trip with a given consumption
    /// </summary>
    /// <param name="obd"></param>
    /// <returns></returns>
    double GetCalculatedTrip(OBD obd);
}


internal class ConsumptionCalculator : IConsumptionCalculator
{
    private readonly IVehicleProvider vehicleProvider;
    private OBD previousObd;

    public ConsumptionCalculator(IVehicleProvider vehicleProvider)
    {
        this.vehicleProvider = vehicleProvider;
    }

    public double GetCalculatedConsumption(OBD obd)
    {
        if (previousObd is null)
        {
            previousObd = obd;
            return 0.0;
        }

        double timeElapsed = (obd.ReadAt - previousObd.ReadAt).TotalSeconds;
        double distanceTraveled = obd.Speed * timeElapsed / 3600;

        previousObd = obd;

        double fuelConsumed = CalculateFuelConsumed(previousObd, obd);
        double fuelConsumption = fuelConsumed / distanceTraveled * 100;

        return fuelConsumption;
    }

    private static double CalculateFuelConsumed(OBD previousObd, OBD currentObd)
    {
        double airFlowRate = (currentObd.MAF / 100) / (currentObd.IntakeAirTemperature + 273.15) * 28.97 / 8.314;
        double fuelFlowRate = currentObd.FuelAirCommanded * airFlowRate / 14.7 / 60;
        double fuelConsumed = fuelFlowRate * (currentObd.TimeSinceTroubleCodesCleared - previousObd.TimeSinceTroubleCodesCleared) / 3600 / 1000 * 0.264172;

        return fuelConsumed;
    }

    public double GetCalculatedTrip(OBD obd)
    {
        double fuelConsumption = GetCalculatedConsumption(obd);

        var currentVehicle = vehicleProvider.Vehicle;

        double remainingFuel = obd.FuelLevel * 0.01 * currentVehicle.FuelCapacity;
        double distanceRemaining = remainingFuel / fuelConsumption * 100;

        return distanceRemaining;
    }
}
