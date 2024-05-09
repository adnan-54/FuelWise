namespace FuelWise.VehicleInformations;

public interface IVehicleRepository
{
    Task<Vehicle?> GetFromVin(string vin);
}

internal class DefaultVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> vehicles;

    public DefaultVehicleRepository()
    {
        vehicles =
        [
            new("Ford", "Fiesta", "9BFZF55PXD8350032-7E80612FF02013942AA\r", "Preto", 2013, 1084, new(1600, 107, 15.8, 7000, 4250, 5500, 4, FuelType.Ethanol, 8.4, 10.4, 100), new([3.58, 1.93, 1.28, 0.95, 0.76], 4.06), new(175, 65, 14)),
        ];
    }

    public Task<Vehicle?> GetFromVin(string vin)
    {
        var vehicle = vehicles.FirstOrDefault(v => v.VIN.Contains(vin, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(vehicle);
    }
}