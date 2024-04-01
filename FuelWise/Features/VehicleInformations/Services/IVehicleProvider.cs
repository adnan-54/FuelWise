using System.Text;
using FuelWise.BluetoothConnection;
using FuelWise.OBDEncoder;

namespace FuelWise.VehicleInformations;

public interface IVehicleProvider
{
    event VehicleChangedEventHandler? VehicleChanged;

    Vehicle? Vehicle { get; }

    Task<Vehicle?> FetchVehicleAsync();
}

internal class DefaultVehicleProvider : IVehicleProvider
{
    private readonly IBluetoothConnector bluetoothConnector;
    private readonly IOBDEncoder encoder;
    private readonly IVehicleRepository vehicleRepository;

    private Vehicle? vehicle;

    public DefaultVehicleProvider(IBluetoothConnector bluetoothConnector, IOBDEncoder encoder, IVehicleRepository vehicleRepository)
    {
        this.bluetoothConnector = bluetoothConnector;
        this.encoder = encoder;
        this.vehicleRepository = vehicleRepository;

        bluetoothConnector.DeviceConnected += OnDeviceConnected;
        bluetoothConnector.DeviceDisconnected += OnDeviceDisconnected;
    }

    public event VehicleChangedEventHandler? VehicleChanged;

    public Vehicle? Vehicle
    {
        get => vehicle;
        set
        {
            vehicle = value;
            VehicleChanged?.Invoke(this, new(vehicle));
        }
    }

    public async Task<Vehicle?> FetchVehicleAsync()
    {
        if (bluetoothConnector.ConnectedDevice is null)
            return null;
        string vin = await GetVinNumber();

        return await vehicleRepository.GetFromVin(vin);
    }

    private async void OnDeviceConnected(object? sender, EventArgs e)
    {
        Vehicle = await FetchVehicleAsync();
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        Vehicle = null;
    }

    private async Task<string> GetVinNumber()
    {
        if (bluetoothConnector.ConnectedDevice is null)
            return string.Empty;

        var request = "0902\r";
        var data = Encoding.ASCII.GetBytes(request);
        await bluetoothConnector.ConnectedDevice.SendAsync(data);
        await Task.Delay(50);
        var response = await bluetoothConnector.ConnectedDevice.ReadAsync();

        var frame = encoder.Decode(response);

        return frame.ToString();
    }
}