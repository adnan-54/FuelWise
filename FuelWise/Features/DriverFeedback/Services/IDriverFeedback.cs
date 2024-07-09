using FuelWise.BluetoothConnection;
using FuelWise.Reporting;

namespace FuelWise.DriverFeedback;

public enum CurrentFeedback
{
    HighThrottle,
    HighGear,
    LowGear,
    HighSpeed,
    HighRpm,
    HighVariation,
    None
}

public interface IDriverFeedback
{
    event FeedbackRecivedEventHandler? FeedbackReceived;
}

public class DefaultDriverFeedback : IDriverFeedback
{
    private readonly IReportGenerator reportGenerator;

    private IDispatcherTimer? timer;

    public DefaultDriverFeedback(IReportGenerator reportGenerator, IBluetoothConnector bluetoothConnection)
    {
        this.reportGenerator = reportGenerator;

        bluetoothConnection.DeviceConnected += OnDeviceConnected;
        bluetoothConnection.DeviceDisconnected += OnDeviceDisconnected;
    }

    public event FeedbackRecivedEventHandler? FeedbackReceived;

    private void OnDeviceConnected(object? sender, EventArgs e)
    {
        if (timer is null)
        {
            timer = Application.Current!.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += OnTimerTick;
        }

        timer.Start();
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        timer?.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (reportGenerator.Reports.Count < 100)
            return;

        var feedback = GenerateFeedback();
        FeedbackReceived?.Invoke(this, new(feedback));
    }

    private CurrentFeedback GenerateFeedback()
    {
        var reports = reportGenerator.Reports.Where(r => r.CreatedAt > DateTime.Now.AddSeconds(-10)).ToList();

        if (reports.Average(r => r.Speed) < 5)
            return CurrentFeedback.None;
        if (reports.Average(r => r.DrivingEfficiency) > 75)
            return CurrentFeedback.None;

        var maxVariation = reports.Max(r => Math.Abs(r.SpeedVariation));
        if (maxVariation > 5)
            return CurrentFeedback.HighVariation;

        var currentGear = reports.Last().Gear;

        var maxThrottle = reports.Max(r => r.ThrottlePosition);
        if (maxThrottle > 65)
        {
            if (currentGear > 2)
                return CurrentFeedback.LowGear;
            else
                return CurrentFeedback.HighThrottle;
        }

        var maxRpm = reports.Max(r => r.Rpm);
        if (maxRpm > 3000)
        {
            if (currentGear < 5)
                return CurrentFeedback.HighGear;
            else
                return CurrentFeedback.HighRpm;
        }

        var maxSpeed = reports.Max(r => r.Speed);
        var isOnHighway = reports.Last().IsOnHighway;
        if (isOnHighway && maxSpeed > 80)
            return CurrentFeedback.HighSpeed;
        else
        if (maxSpeed > 60)
            return CurrentFeedback.HighSpeed;

        return CurrentFeedback.None;
    }
}

public delegate void FeedbackRecivedEventHandler(object sender, FeedbackReceivedEventArgs e);

public sealed class FeedbackReceivedEventArgs : EventArgs
{
    public FeedbackReceivedEventArgs(CurrentFeedback feedback)
    {
        Feedback = feedback;
    }

    public CurrentFeedback Feedback { get; }
}