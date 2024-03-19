using System.Text;
using Android.Bluetooth;
using FuelWise.Services;
using Java.Util;

namespace FuelWise.Platforms.Android;

public class AndroidBluetoothConnector : IBluetoothConnector
{
    private const string SspUdid = "00001101-0000-1000-8000-00805f9b34fb";
    private readonly BluetoothAdapter adapter;
    private BluetoothSocket? socket;

    public AndroidBluetoothConnector()
    {
        if(MauiApplication.Current.GetSystemService("bluetooth") is not BluetoothManager manager)
            throw new System.Exception("Bluetooth manager not found.");

        adapter = manager.Adapter ?? throw new System.Exception("Bluetooth adapter not found.");
    }

    public IEnumerable<string> GetConnectedDevices()
    {
        if (adapter.IsEnabled)
        {
            if (adapter.BondedDevices is not null && adapter.BondedDevices.Count > 0)
                return adapter.BondedDevices.Select(d => d.Name ?? "Unknow Device");
        }

        return Enumerable.Empty<string>();
    }

    public void Connect(string deviceName)
    {
        var device = (adapter.BondedDevices?.FirstOrDefault(d => d.Name == deviceName)) ?? throw new System.Exception($"Device '{deviceName}' not found.");
        socket = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(SspUdid)) ?? throw new System.Exception($"Could not create socket for device '{deviceName}'");
        socket.Connect();
    }

    public void TurnOn()
    {
        var buffer = "1";
        socket?.OutputStream?.WriteAsync(Encoding.ASCII.GetBytes(buffer), 0, buffer.Length);
    }

    public void TurnOff()
    {
        var buffer = "2";
        socket?.OutputStream?.WriteAsync(Encoding.ASCII.GetBytes(buffer), 0, buffer.Length);
    }

    public bool IsConnected()
    {
        return adapter.BondedDevices?.Count > 0; ;
    }

    public void Disconnect()
    {
        socket?.OutputStream?.Close();
        socket?.Close();
        socket = null;
    }
}

