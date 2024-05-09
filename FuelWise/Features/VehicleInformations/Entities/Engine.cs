namespace FuelWise.VehicleInformations;

public record Engine(int Displacement, int Horsepower, double Torque, int RedlineRpm, int TorqueRpm, int HorsepowerRpm, int Cylinders, FuelType FuelType, double UrbanConsumption, double HighwayConsumption, double OperatingTemperature)
{
    public double GetAirFuelRatio()
    {
        if (FuelType == FuelType.Ethanol)
            return 9.0;
        if (FuelType == FuelType.Gasoline)
            return 14.7;

        return (9 + 14.7) / 2;
    }
}