using System.Text;
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

    public Task Send(string data)
    {
        var buffer = Encoding.ASCII.GetBytes(data);
        return outputStream.WriteAsync(buffer, 0, data.Length);
    }

    public async Task<string> Read()
    {
        var output = new byte[16];
        var size = await inputStream.ReadAsync(output.AsMemory(0));

        Array.Resize(ref output, size);

        var value = Encoding.ASCII.GetString(output);

        return value;

    }
}
