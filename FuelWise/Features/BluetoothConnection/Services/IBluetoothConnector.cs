namespace FuelWise.BluetoothConnection;

public interface IBluetoothConnector
{
    IBluetoothDevice? ConnectedDevice { get; }

    Task<IEnumerable<string>> GetAvailableDevices();

    Task<IBluetoothDevice> Connect(string deviceName);

    Task Disconnect();
}
