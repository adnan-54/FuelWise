using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuelWise.BluetoothConnection;
using FuelWise.NativeDialog;

namespace FuelWise.ViewModels;

public enum ConnectionState
{
    Searching,
    DevicesFound,
    DevicesNotFound,
    Connecting,
    Connected,
    Disconecting,
    Disconnected,
    Error
}

public partial class ConnectionViewModel : ObservableObject
{
    private readonly IBluetoothConnector btConnector;
    private readonly IDialogManager dialogManager;

    [ObservableProperty]
    private List<string>? availableDevices;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConnect))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string? selectedDevice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDisconnect))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    private string? connectedDevice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSearchDevices))]
    [NotifyCanExecuteChangedFor(nameof(SearchDevicesCommand))]
    private ConnectionState currentState;

    public ConnectionViewModel(IBluetoothConnector btConnector, IDialogManager dialogManager)
    {
        this.btConnector = btConnector;
        this.dialogManager = dialogManager;

        CurrentState = ConnectionState.Disconnected;
    }

    public bool CanSearchDevices => CurrentState is ConnectionState.Disconnected or ConnectionState.DevicesNotFound or ConnectionState.Error;

    public bool CanConnect => !string.IsNullOrWhiteSpace(SelectedDevice);

    public bool CanDisconnect => !string.IsNullOrWhiteSpace(ConnectedDevice);

    [RelayCommand(CanExecute = nameof(CanSearchDevices))]
    public async Task SearchDevices()
    {
        try
        {
            CurrentState = ConnectionState.Searching;

            SelectedDevice = null;

            var devices = await btConnector.GetAvailableDevices();
            AvailableDevices = new(devices);

            await Task.Delay(TimeSpan.FromSeconds(2));

            if (AvailableDevices.Any())
            {
                CurrentState = ConnectionState.DevicesFound;
                SelectedDevice = AvailableDevices.FirstOrDefault(d => d.Contains("OBD", StringComparison.OrdinalIgnoreCase));
            }
            else
                CurrentState = ConnectionState.DevicesNotFound;
        }
        catch (Exception ex)
        {
            CurrentState = ConnectionState.Error;
            await dialogManager.ShowError(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    public async Task Connect()
    {
        try
        {
            if (SelectedDevice is null)
                return;

            CurrentState = ConnectionState.Connecting;

            await btConnector.Connect(SelectedDevice);
            await Task.Delay(TimeSpan.FromSeconds(2));

            ConnectedDevice = SelectedDevice;
            SelectedDevice = null;

            CurrentState = ConnectionState.Connected;

            await dialogManager.ShowSuccess("Dispositivo conectado com sucesso");
        }
        catch (Exception ex)
        {
            CurrentState = ConnectionState.Error;
            await dialogManager.ShowError(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    public async Task Disconnect()
    {
        try
        {
            if (ConnectedDevice is null)
                return;

            CurrentState = ConnectionState.Disconecting;

            await btConnector.Disconnect();
            await Task.Delay(TimeSpan.FromSeconds(2));

            ConnectedDevice = null;

            CurrentState = ConnectionState.Disconnected;

            await dialogManager.ShowSuccess("Dispositivo desconectado com sucesso");
        }
        catch (Exception ex)
        {
            CurrentState = ConnectionState.Error;
            await dialogManager.ShowError(ex.Message);
        }
    }
}