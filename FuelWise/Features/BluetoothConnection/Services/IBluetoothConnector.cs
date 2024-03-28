namespace FuelWise.BluetoothConnection;

public interface IBluetoothConnector
{
    event EventHandler? DeviceConnected;

    event EventHandler? DeviceDisconnected;

    event EventHandler? DeviceChanged;

    IBluetoothDevice? ConnectedDevice { get; }

    bool IsConnected { get; }

    Task<IEnumerable<string>> GetAvailableDevices();

    Task<IBluetoothDevice> Connect(string deviceName);

    Task Disconnect();
}