using FuelWise.Models;

namespace FuelWise.Services;

public interface IEfficiencyCalculator
{
    double GetCalculatedEficiency(OBD obd);
}

internal class EfficiencyCalculator : IEfficiencyCalculator
{
    private readonly IVehicleProvider vehicleProvider;
    private readonly IConsumptionCalculator consumptionCalculator;

    public EfficiencyCalculator(IVehicleProvider vehicleProvider, IConsumptionCalculator consumptionCalculator)
    {
        this.vehicleProvider = vehicleProvider;
        this.consumptionCalculator = consumptionCalculator;
    }

    public double GetCalculatedEficiency(OBD obd)
    {
        var possibleDistance = consumptionCalculator.GetCalculatedTrip(obd);
        double maxDistance = vehicleProvider.Vehicle.Autonomy;

        return possibleDistance / maxDistance;
    }
}
