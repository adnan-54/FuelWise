using Android.Bluetooth;
using FuelWise.Platforms.Android.Bluetooth;
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

    public IBluetoothDevice? ConnectedDevice => connectedDevice;

    public async Task<IEnumerable<string>> GetAvailableDevices()
    {
        await CheckPermission();

        if (adapter.IsEnabled)
        {
            if (adapter.BondedDevices is not null && adapter.BondedDevices.Count > 0)
                return adapter.BondedDevices.Select(d => d.Name ?? "Dispositivo Desconhecido");
        }

        return Enumerable.Empty<string>();
    }

    public async Task<IBluetoothDevice> Connect(string deviceName)
    {
        var device = (adapter.BondedDevices?.FirstOrDefault(d => d.Name == deviceName)) ?? throw new Exception($"Dispositivo '{deviceName}' não encontrado.");
        var socket = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(SspUdid)) ?? throw new Exception($"Não foi possivel criar um soket para o dispositivo '{deviceName}'");
        await socket.ConnectAsync();
        connectedDevice = new AndroidBluetoothDevice(device, socket);

        return connectedDevice;
    }

    public async Task Disconnect()
    {
        if (connectedDevice is null)
            return;

        try
        {
            var socket = connectedDevice.BluetoothSocket;

            if(socket.InputStream is not null)
                await socket.InputStream.DisposeAsync();

            if (socket.OutputStream is not null)
                await socket.OutputStream.DisposeAsync();
        }
        finally
        {
            connectedDevice = null;
        }
    }

    private static async Task<bool> CheckPermission()
    {
        var status = await Permissions.CheckStatusAsync<BluetoothPermissions>();

        if (status != PermissionStatus.Granted)
            await Application.Current!.MainPage!.DisplayAlert("Permissão Bluetooth", "Permita a conexão via Bluetooth", "OK");

        status = await Permissions.RequestAsync<BluetoothPermissions>();

        if (status != PermissionStatus.Granted)
            throw new Exception("Permissão bluetooth negada");

        return status == PermissionStatus.Granted;
    }
}

