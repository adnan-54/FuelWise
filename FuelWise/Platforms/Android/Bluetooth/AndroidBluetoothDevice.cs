using Android.Bluetooth;
using FuelWise.BluetoothConnection;

namespace FuelWise.Platforms.Android;

public class AndroidBluetoothDevice : IBluetoothDevice
{
    private readonly BluetoothDevice bluetoothDevice;
    private readonly Stream outputStream;
    private readonly Stream inputStream;

    public AndroidBluetoothDevice(BluetoothDevice bluetoothDevice, BluetoothSocket bluetoothSocket)
    {
        this.bluetoothDevice = bluetoothDevice;
        BluetoothSocket = bluetoothSocket;
        outputStream = bluetoothSocket.OutputStream ?? throw new Exception("Não foi possivel criar o canal de saida");
        inputStream = bluetoothSocket.InputStream ?? throw new Exception("Não foi possivel criar o canal de entrada");
    }

    public string Name => bluetoothDevice.Name ?? "Dispositivo Desconhecido";

    public BluetoothSocket BluetoothSocket { get; }

    public async Task SendAsync(byte[] data, CancellationToken cancellationToken)
    {
        await outputStream.FlushAsync(cancellationToken);
        await outputStream.WriteAsync(data, cancellationToken);
    }

    public async Task<byte[]> ReadAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[128];
        var size = await inputStream.ReadAsync(buffer, cancellationToken);

        while (buffer[size - 1] != 62)
        {
            var tempBuffer = new byte[128];
            var tempSize = await inputStream.ReadAsync(tempBuffer, cancellationToken);

            Array.Copy(tempBuffer, 0, buffer, size, tempSize);
            size += tempSize;
        }

        await inputStream.FlushAsync(cancellationToken);

        Array.Resize(ref buffer, size);

        return buffer;
    }
}