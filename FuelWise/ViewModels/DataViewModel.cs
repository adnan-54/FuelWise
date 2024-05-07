using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuelWise.BluetoothConnection;
using FuelWise.Reporting;
using FuelWise.Views;

namespace FuelWise.ViewModels;

public enum DataState
{
    NotConnected,
    ViewData,
    VehicleMoving,
}

public partial class DataViewModel : ObservableObject
{
    [ObservableProperty]
    private DataState currentState;

    [ObservableProperty]
    private string? speed;

    [ObservableProperty]
    private string? averageSpeed;

    [ObservableProperty]
    private string? speedVariation;

    [ObservableProperty]
    private string? rpm;

    [ObservableProperty]
    private string? coolantTemperature;

    [ObservableProperty]
    private string? engineLoad;

    [ObservableProperty]
    private string? intakeAirTemperature;

    [ObservableProperty]
    private string? intakePressure;

    [ObservableProperty]
    private string? throttlePosition;

    [ObservableProperty]
    private string? gear;

    [ObservableProperty]
    private string? instantFuelComsumption;

    [ObservableProperty]
    private string? averageFuelComsumption;

    [ObservableProperty]
    private string? massAirFlow;

    [ObservableProperty]
    private string? volumetricEfficiency;

    [ObservableProperty]
    private string? drivingStyle;

    [ObservableProperty]
    private string? drivingEfficiency;

    [ObservableProperty]
    private string? vehicleMoving;

    [ObservableProperty]
    private string? engineRunning;

    [ObservableProperty]
    private string? reportGenerationTime;

    private Report? lastReport;

    public DataViewModel(IBluetoothConnector bluetoothConnector, IReportGenerator reportGenerator)
    {
        bluetoothConnector.DeviceConnected += OnDeviceConnected;
        bluetoothConnector.DeviceDisconnected += OnDeviceDisconnected;
        reportGenerator.ReportGenerated += OnReportGenerated;

        CurrentState = DataState.NotConnected;
    }

    private void OnReportGenerated(object sender, ReportGeneratedEventArgs e)
    {
        var report = e.Report;

        if (CurrentState == DataState.NotConnected)
            return;

        if (report.IsVehicleMoving)
        {
            //CurrentState = DataState.VehicleMoving;
            //return;
        }
        else if (CurrentState != DataState.ViewData)
            CurrentState = DataState.ViewData;

        ReportGenerationTime = (DateTime.Now - (lastReport is null ? DateTime.Now : lastReport.CreatedAt)).TotalMilliseconds.ToString("0");

        Speed = report.Speed.ToString("0");
        AverageSpeed = report.AverageSpeed.ToString("0");
        SpeedVariation = report.SpeedVariation.ToString("0");

        Rpm = report.Rpm.ToString("0");

        CoolantTemperature = report.CoolantTemperature.ToString("0");

        EngineLoad = report.EngineLoad.ToString("0");

        IntakeAirTemperature = report.IntakeAirTemperature.ToString("0");

        IntakePressure = report.IntakePressure.ToString("0.00");

        ThrottlePosition = report.ThrottlePosition.ToString("0");

        Gear = report.Gear == 0 ? "N" : report.Gear.ToString("0");

        InstantFuelComsumption = report.FuelConsumption.ToString("0.00");
        AverageFuelComsumption = report.AverageFuelConsumption.ToString("0.00");

        MassAirFlow = report.MassAirFlow.ToString("0.00");

        VolumetricEfficiency = report.VolumetricEfficiency.ToString("0");

        DrivingStyle = report.DrivingStyle == IA.DrivingStyle.Even ? "Contínuo" : "Agressivo";

        DrivingEfficiency = report.DrivingEfficiency.ToString("0");

        VehicleMoving = report.IsVehicleMoving ? "Sim" : "Não";

        EngineRunning = report.IsEngineRunning ? "Sim" : "Não";

        lastReport = report;
    }

    [RelayCommand]
    public void NavigateToConnection()
    {
        if (Application.Current is not null && Application.Current.MainPage is MainPage mainPage)
            mainPage.NavigateToDataView();
    }

    private void OnDeviceConnected(object? sender, EventArgs e)
    {
        CurrentState = DataState.ViewData;
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        CurrentState = DataState.NotConnected;
    }
}