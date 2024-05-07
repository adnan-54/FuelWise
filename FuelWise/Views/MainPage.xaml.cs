using FuelWise.BluetoothConnection;
using FuelWise.ViewModels;

namespace FuelWise;

public partial class MainPage : TabbedPage
{
    private bool firstLoaded;

    public MainPage(MainPageViewModel mainPageViewModel, IBluetoothConnector bluetoothConnector)
    {
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
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        NavigateToConnection();
        Application.Current!.MainPage = Children.Last();
    }
}