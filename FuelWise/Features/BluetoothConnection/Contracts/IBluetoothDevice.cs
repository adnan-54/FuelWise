namespace FuelWise;

public interface IBluetoothDevice
{
    string Name { get; }

    Task Send(string data);

    Task<string> Read();
}