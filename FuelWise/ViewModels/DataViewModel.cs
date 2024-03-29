using System.Diagnostics;
using System.Text;
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
            //var speed = await dataPuller.PullDataAsync<VehicleSpeedData>();
            //var coolantData = await dataPuller.PullDataAsync<EngineCoolantTemperatureData>();

            //volumetric efficiency
            //2.82/(873*1.07*(1.6/120))
            //maf/(rpm*density of air*(displace/120))

            var engineLoadData = await dataPuller.PullDataAsync<EngineLoadData>();
            var rpmData = await dataPuller.PullDataAsync<RpmData>();
            var mapData = await dataPuller.PullDataAsync<IntakeManifoldPressureData>();
            var intakeAirTempData = await dataPuller.PullDataAsync<IntakeAirTemperatureData>();
            var throttlePositionData = await dataPuller.PullDataAsync<ThrottlePositionData>();

            MassAirFlow.ModelInput input = new()
            {
                EngineLoad = engineLoadData.Value,
                RPM = rpmData.Value,
                IntakeManifoldPressure = mapData.Value,
                IntakeAirTemperature = intakeAirTempData.Value,
                ThrottlePosition = throttlePositionData.Value
            };

            var result = MassAirFlow.Predict(input);
            var estimatedMaf = result.Score;

            var volumetricEfficiency = estimatedMaf / (rpmData.Value * 1.07 * (1.6 / 120));

            var imap = rpmData.Value * mapData.Value / (intakeAirTempData.ToKelvin() / 2);
            var maf = imap / 60 * volumetricEfficiency * 1.6 * 3.484484;

            var sb = new StringBuilder();
            sb.AppendLine($"Estimated MAF: {estimatedMaf}");
            sb.AppendLine($"Volumetric Efficiency: {volumetricEfficiency}");
            sb.AppendLine($"IMAP: {imap}");
            sb.AppendLine($"Calculated MAF: {maf}");

            Report = sb.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}