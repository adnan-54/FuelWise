using FuelWise.BluetoothConnection;
using FuelWise.TemperatureAlert;
using FuelWise.ViewModels;

namespace FuelWise;

public partial class MainPage : TabbedPage
{
    private readonly ITemperatureAlert temperatureAlert;
    private bool firstLoaded;

    public MainPage(MainPageViewModel mainPageViewModel, IBluetoothConnector bluetoothConnector, ITemperatureAlert temperatureAlert)
    {
        this.temperatureAlert = temperatureAlert;

        firstLoaded = true;

        InitializeComponent();

        BindingContext = mainPageViewModel;

        Loaded += OnLoaded;

        bluetoothConnector.DeviceConnected += OnDeviceConnected;
        bluetoothConnector.DeviceDisconnected += OnDeviceDisconnected;
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