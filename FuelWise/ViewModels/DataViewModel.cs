using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuelWise.BluetoothConnection;
using FuelWise.OBDDataPuller;
using FuelWise_IA;

namespace FuelWise.ViewModels;

public partial class DataViewModel : ObservableObject
{
    private readonly IBluetoothConnector bluetoothConnector;
    private readonly IDataPuller dataPuller;
    private bool isGeneratingReport;

    [ObservableProperty]
    private string? report;

    public DataViewModel(IBluetoothConnector bluetoothConnector, IDataPuller dataPuller)
    {
        this.bluetoothConnector = bluetoothConnector;
        this.dataPuller = dataPuller;
    }

    [RelayCommand]
    public async Task RequestData()
    {
        if (!bluetoothConnector.IsConnected)
            return;

        try
        {
            var speed = await dataPuller.PullDataAsync<VehicleSpeedData>();
            var coolantData = await dataPuller.PullDataAsync<EngineCoolantTemperatureData>();
            var rpmData = await dataPuller.PullDataAsync<RpmData>();
            var intakeAirTempData = await dataPuller.PullDataAsync<IntakeAirTemperatureData>();
            var engineLoadData = await dataPuller.PullDataAsync<EngineLoadData>();
            var throttlePositionData = await dataPuller.PullDataAsync<ThrottlePositionData>();
            var mapData = await dataPuller.PullDataAsync<IntakeManifoldPressureData>();

            var sampleData = new MassAirFlow.ModelInput()
            {
                Speed = speed.ToMeterPerSecond(),
                Altitude = 500F,
                CoolantTemperature = coolantData.Value,
                RPM = rpmData.Value,
                IntakeAirTemperature = intakeAirTempData.Value,
                EngineLoad = engineLoadData.Value,
                ThrottlePosition = throttlePositionData.Value,
            };

            var result = MassAirFlow.Predict(sampleData);

            //volumetric efficiency
            //2.82/(873*1.07*(1.6/120))
            //maf/(rpm*density of air*(displace/120))

            var imap = rpmData.Value * mapData.Value / intakeAirTempData.ToKelvin() / 2;
            var gramsOfAir = imap * 5.13806818;

            Report = $"Calculated MAF: {gramsOfAir}g/s{Environment.NewLine}Predicted MAF: {result.Score}g/s";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}