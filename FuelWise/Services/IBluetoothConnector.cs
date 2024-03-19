namespace FuelWise.Services;

public interface IBluetoothConnector
{
    IEnumerable<string> GetConnectedDevices();

    void Connect(string deviceName);

    void TurnOn();

    void TurnOff();

    bool IsConnected();

    void Disconnect();
}


