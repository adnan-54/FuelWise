using FuelWise.BluetoothConnection;
using FuelWise.Reporting;
using FuelWise.TemperatureAlert;
using FuelWise.ViewModels;
using Application = Microsoft.Maui.Controls.Application;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace FuelWise;

public partial class MainPage : TabbedPage
{
    private readonly ITemperatureAlert temperatureAlert;
    private bool firstLoaded;
    private bool lastIsMoving;

    public MainPage(MainPageViewModel mainPageViewModel, IBluetoothConnector bluetoothConnector, ITemperatureAlert temperatureAlert, IReportGenerator reportGenerator)
    {
        this.temperatureAlert = temperatureAlert;

        firstLoaded = true;

        InitializeComponent();

        BindingContext = mainPageViewModel;

        Loaded += OnLoaded;

        bluetoothConnector.DeviceConnected += OnDeviceConnected;
        bluetoothConnector.DeviceDisconnected += OnDeviceDisconnected;

#if RELEASE
        reportGenerator.ReportGenerated += OnReportGenerated;
#endif
    }

    private void OnReportGenerated(object sender, ReportGeneratedEventArgs e)
    {
        var isMoving = e.Report.IsVehicleMoving;
        if (lastIsMoving != isMoving)
            IsMovingChanged(isMoving);
    }

    private void IsMovingChanged(bool isMoving)
    {
        var application = Application.Current;
        if (application is null)
            return;

        lastIsMoving = isMoving;

        if (!isMoving)
        {
            NavigateToHome();
            application.MainPage = this;
        }
        else
            application.MainPage = Children.First();
    }

    public void NavigateToHome()
    {
        CurrentPage = Children.First();
    }

    public void NavigateToConnection()
    {
        CurrentPage = Children.Last();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (!firstLoaded)
            return;
        firstLoaded = false;

        Application.Current!.MainPage = Children.Last();
    }

    private void OnDeviceConnected(object? sender, EventArgs e)
    {
        NavigateToHome();
        Application.Current!.MainPage = this;
        DeviceDisplay.Current.KeepScreenOn = true;
        temperatureAlert.Start();
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        NavigateToConnection();
        Application.Current!.MainPage = Children.Last();
        DeviceDisplay.Current.KeepScreenOn = false;
        temperatureAlert.Stop();
    }
}