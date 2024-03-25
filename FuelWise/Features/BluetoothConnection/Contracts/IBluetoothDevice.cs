namespace FuelWise.BluetoothConnection;

public interface IBluetoothDevice
{
    string Name { get; }

    Task SendAsync(byte[] data, CancellationToken cancellation = default);

    Task<byte[]> ReadAsync(CancellationToken cancellation = default);
}