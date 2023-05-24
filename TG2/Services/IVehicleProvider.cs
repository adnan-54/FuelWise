using TG2.Models;

namespace TG2.Services;

public interface IVehicleProvider
{
    Vehicle Vehicle { get; }

    Engine Engine { get; }

    Transmission Transmission { get; }

    Wheel Wheel { get; }

    Task<Vehicle> UpdateVehicle();
}

internal class VehicleProvider : IVehicleProvider
{
    public Vehicle Vehicle { get; set; }

    public Engine Engine => Vehicle.Engine;

    public Transmission Transmission => Vehicle.Transmission;

    public Wheel Wheel => Vehicle.Wheel;

    public async Task<Vehicle> UpdateVehicle()
    {
        return Vehicle ??= await VehicleProvider.FetchCurrentVehicle();
    }

    private static Task<Vehicle> FetchCurrentVehicle()
    {
        var engine = new Engine("AP", 1800, 4, 106, 5200, 16, 3000);
        var gearRatios = new[] { 3.455, 1.944, 1.286, 0.969, 0.8 };
        var transmission = new Transmission(5, gearRatios, 4.111);
        var wheel = new Wheel(14, 185, 65);
        var vehicle = new Vehicle("Volkswagem", "Gol", "Power TotalFlex", "9BWCC05X25P145295", 2005, 51, 1104, engine, transmission, wheel, 459);

        return Task.FromResult(vehicle);
    }
}