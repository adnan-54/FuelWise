using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using FuelWise.BluetoothConnection;
using FuelWise.OBDEncoder;

namespace FuelWise.OBDDataPuller;

public interface IRequestProcessor
{
    event FrameReceivedEventHandler FrameReceived;

    void QueueRequest<TType>(TType request) where TType : class, IOBDData;
}

internal class DefaultRequestProcessor : IRequestProcessor
{
    private static readonly TimeSpan OBD_INTERVAL = TimeSpan.FromMilliseconds(10);

    private readonly IBluetoothConnector bluetoothConnector;
    private readonly IOBDEncoder encoder;
    private readonly BufferBlock<IOBDData> dataQueue;

    private Task? workerTask;
    private CancellationTokenSource? cts;

    public DefaultRequestProcessor(IBluetoothConnector bluetoothConnector, IOBDEncoder encoder)
    {
        this.bluetoothConnector = bluetoothConnector;
        this.encoder = encoder;
        dataQueue = new();

        bluetoothConnector.DeviceConnected += OnDeviceConnected;
        bluetoothConnector.DeviceDisconnected += OnDeviceDisconnected;
    }

    public event FrameReceivedEventHandler? FrameReceived;

    void IRequestProcessor.QueueRequest<TType>(TType request)
    {
        dataQueue.SendAsync(request);
    }

    private async Task Process()
    {
        while (IsProcessing())
        {
            try
            {
                var cancelationToken = cts!.Token;

                var request = await dataQueue.ReceiveAsync(cancelationToken);

                var buffer = encoder.Encode(request.Frame);
                cancelationToken.ThrowIfCancellationRequested();

                var connectedDevice = bluetoothConnector.ConnectedDevice;
                if (connectedDevice is null)
                    continue;

                await connectedDevice.SendAsync(buffer, cancelationToken);
                await Task.Delay(OBD_INTERVAL, cancelationToken);
                var result = await connectedDevice.ReadAsync(cancelationToken);

                var frame = encoder.Decode(result);
                FrameReceived?.Invoke(this, new(frame, request));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }

    private void OnDeviceConnected(object? sender, EventArgs e)
    {
        StartBackgroundTask();
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        StopBackgroundTask();
    }

    private void StartBackgroundTask()
    {
        if (IsProcessing())
            return;

        cts = new CancellationTokenSource();
        workerTask = Task.Run(Process, cts.Token);
    }

    private void StopBackgroundTask()
    {
        if (!IsProcessing())
            return;

        cts?.CancelAsync().ContinueWith((_) =>
        {
            cts?.Dispose();
            cts = null;
        });

        workerTask?.ContinueWith((_) =>
        {
            workerTask?.Dispose();
            workerTask = null;
        });

        dataQueue.ReceiveAllAsync();
    }

    private bool IsProcessing()
    {
        return workerTask is not null
               && cts is not null
               && workerTask.Status != TaskStatus.RanToCompletion
               && workerTask.Status != TaskStatus.Faulted
               && workerTask.Status != TaskStatus.Canceled
               && !cts.IsCancellationRequested;
    }
}