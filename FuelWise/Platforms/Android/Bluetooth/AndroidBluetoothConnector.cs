using Android.Bluetooth;
using FuelWise.BluetoothConnection;
using Java.Util;

namespace FuelWise.Platforms.Android;

public class AndroidBluetoothConnector : IBluetoothConnector
{
    private const string SspUdid = "00001101-0000-1000-8000-00805f9b34fb";

    private readonly BluetoothAdapter adapter;
    private AndroidBluetoothDevice? connectedDevice;

    public AndroidBluetoothConnector()
    {
        if (MauiApplication.Current.GetSystemService("bluetooth") is not BluetoothManager manager)
            throw new Exception("Gerenciador bluetooth não encontrado");

        adapter = manager.Adapter ?? throw new Exception("Adaptador bluetooth não encontrado");
    }

    public event EventHandler? DeviceConnected;

    public event EventHandler? DeviceDisconnected;

    public event EventHandler? DeviceChanged;

    public IBluetoothDevice? ConnectedDevice
    {
        get => connectedDevice;
        private set
        {
            if (value is null)
                DeviceDisconnected?.Invoke(this, EventArgs.Empty);
            else
                DeviceConnected?.Invoke(this, EventArgs.Empty);

            connectedDevice = (AndroidBluetoothDevice?)value;

            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsConnected => ConnectedDevice != null;

    public async Task<IEnumerable<string>> GetAvailableDevices()
    {
        await CheckPermission();

        if (adapter.IsEnabled)
        {
            if (adapter.BondedDevices is not null && adapter.BondedDevices.Count > 0)
                return adapter.BondedDevices.Select(d => d.Name ?? "Dispositivo Desconhecido");
        }

        return [];
    }

    public async Task<IBluetoothDevice> Connect(string deviceName)
    {
        var device = (adapter.BondedDevices?.FirstOrDefault(d => d.Name == deviceName)) ?? throw new Exception($"Dispositivo '{deviceName}' não encontrado.");
        var socket = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(SspUdid)) ?? throw new Exception($"Não foi possivel criar um soket para o dispositivo '{deviceName}'");
        await socket.ConnectAsync();
        ConnectedDevice = new AndroidBluetoothDevice(device, socket);

        return ConnectedDevice;
    }

    public async Task Disconnect()
    {
        if (connectedDevice is null)
            return;

        try
        {
            var socket = connectedDevice.BluetoothSocket;

            if (socket.InputStream is not null)
                await socket.InputStream.DisposeAsync();

            if (socket.OutputStream is not null)
                await socket.OutputStream.DisposeAsync();
        }
        finally
        {
            ConnectedDevice = null;
        }
    }

    private static async Task<bool> CheckPermission()
    {
        var status = await Permissions.CheckStatusAsync<AndroidBluetoothPermissions>();

        if (status != PermissionStatus.Granted)
            await Application.Current!.MainPage!.DisplayAlert("Permissão Bluetooth", "Permita a conexão via Bluetooth", "OK");

        status = await Permissions.RequestAsync<AndroidBluetoothPermissions>();

        if (status != PermissionStatus.Granted)
            throw new Exception("Permissão bluetooth negada");

        return status == PermissionStatus.Granted;
    }
}