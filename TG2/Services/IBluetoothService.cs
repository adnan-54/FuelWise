using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace TG2.Services;

public interface IBluetoothService
{
    IReadOnlyList<IDevice> AvailableDevices { get; }

    bool IsConnecting { get; }

    bool IsConnected { get; }

    Task<IReadOnlyCollection<IDevice>> ScanDevices();

    Task Connect();

    Task Disconnect();
}

internal class BluetoothService : IBluetoothService
{
    private readonly IAdapter adapter;
    private readonly List<IDevice> devices;
    private IDevice? currentDevice;

    public BluetoothService(IBluetoothLE bluetoothLE, IAdapter adapter)
    {
        this.adapter = adapter;
        devices = new();

        bluetoothLE.StateChanged += OnStateChanged;
        adapter.DeviceDiscovered += OnDeviceDiscovered;
        adapter.ScanMode = ScanMode.LowLatency;
        adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;
    }

    public IReadOnlyList<IDevice> AvailableDevices => devices;

    public bool IsConnecting { get; private set; }

    public bool IsConnected => currentDevice is not null;

    public async Task<IReadOnlyCollection<IDevice>> ScanDevices()
    {
        devices.Clear();

        await EnsurePermissions();
        await adapter.StartScanningForDevicesAsync();

        return AvailableDevices;
    }

    public async Task Connect()
    {
        if (IsConnecting || IsConnected)
            return;

        try
        {
            IsConnecting = true;
            await ConnectCore();
        }
        catch
        {
            throw;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private async Task ConnectCore()
    {
        await EnsurePermissions();
        var availableDevices = await ScanDevices();

        currentDevice = availableDevices.FirstOrDefault(d => d.Name == "OBD II");

        if (currentDevice is null)
            return;

        await adapter.ConnectToDeviceAsync(currentDevice);
    }

    public async Task Disconnect()
    {
        if (IsConnecting || !IsConnected)
            return;

        devices.Clear();

        await adapter.DisconnectDeviceAsync(currentDevice);
        currentDevice = null;
    }

    private void OnStateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        if (e.NewState == BluetoothState.Off)
        {
            IsConnecting = false;
            currentDevice = null;
        }
    }

    private void OnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        devices.Add(e.Device);
    }

    private static async Task EnsurePermissions()
    {
        var status = await Permissions.CheckStatusAsync<BluetoothPermissions>();

        if (status == PermissionStatus.Granted)
            return;

        if (Permissions.ShouldShowRationale<BluetoothPermissions>())
            await Shell.Current.DisplayAlert("Bluetooth permissions", "Bluetooth permissions are needed to scan for devices.", "Ok");

        await Permissions.RequestAsync<BluetoothPermissions>();
    }
}