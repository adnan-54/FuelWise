using FuelWise.Events;
using FuelWise.Models;

namespace FuelWise.Services;

public interface IDataPuller
{
    TimeSpan PullingInterval { get; }

    event EventHandler<DataPulledEventArgs> DataPulled;

    void StartPulling();

    void StopPulling();

    void SetPullingInterval(TimeSpan newInterval);
}

internal class ControlledDataPuller : IDataPuller
{
    private bool isPullingData;
    private CancellationTokenSource? cancellationTokenSource;
    private TimeSpan pullingInterval;

    public ControlledDataPuller()
    {
        pullingInterval = TimeSpan.FromSeconds(0.5);
    }

    public TimeSpan PullingInterval => pullingInterval;

    public event EventHandler<DataPulledEventArgs>? DataPulled;

    public void StartPulling()
    {
        if (isPullingData)
            return;

        isPullingData = true;
        cancellationTokenSource = new CancellationTokenSource();

        Task.Run(() => PullData(cancellationTokenSource.Token), cancellationTokenSource.Token);
    }

    public void StopPulling()
    {
        if (!isPullingData)
            return;

        isPullingData = false;
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    public void SetPullingInterval(TimeSpan newInterval)
    {
        if (newInterval == pullingInterval)
            return;

        pullingInterval = newInterval;
    }

    private async Task PullData(CancellationToken cancellationToken)
    {
        while (isPullingData)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var data = new OBD(0, 0, 0, 0, 0, 0, 0, DateTime.Now);
            DataPulled?.Invoke(this, new DataPulledEventArgs(data));

            await Task.Delay(pullingInterval, cancellationToken);
        }
    }
}