using FuelWise.Reporting;
using FuelWise.VehicleInformations;
using Plugin.Maui.Audio;

namespace FuelWise.TemperatureAlert;

public interface ITemperatureAlert
{
    void Start();

    void Stop();
}

internal class DefaultTemperatureAlert : ITemperatureAlert
{
    private readonly IVehicleProvider vehicleProvider;
    private readonly IAudioManager audioManager;

    private IAudioPlayer? player;
    private IDispatcherTimer? timer;
    private bool isOverheating;

    public DefaultTemperatureAlert(IReportGenerator reportGenerator, IVehicleProvider vehicleProvider, IAudioManager audioManager)
    {
        this.vehicleProvider = vehicleProvider;
        this.audioManager = audioManager;

        reportGenerator.ReportGenerated += OnReportGenerated;
    }

    public void Start()
    {
        if (timer is not null)
            return;

        var track = FileSystem.OpenAppPackageFileAsync("chime.wav").GetAwaiter().GetResult();
        player = audioManager.CreatePlayer(track);

        timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1.0 / 4);
        timer.Tick += OnTimerTick;
        timer.Start();
    }

    public void Stop()
    {
        if (timer is null)
            return;

        timer.Stop();
        timer = null;
    }

    private void OnReportGenerated(object sender, ReportGeneratedEventArgs e)
    {
        var engine = vehicleProvider.Vehicle?.Engine;

        if (engine is null)
            return;

        var currentTemperature = e.Report.CoolantTemperature;
        var maxTemperature = engine.OperatingTemperature + 10;

        isOverheating = currentTemperature > maxTemperature;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!isOverheating || player is null || player.IsPlaying)
            return;

        player.Stop();
        player.Play();
    }
}