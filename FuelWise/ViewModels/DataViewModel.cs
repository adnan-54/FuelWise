using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuelWise.BluetoothConnection;
using FuelWise.IA;
using FuelWise.OBDDataPuller;
using FuelWise.WiseCalculations;
using FuelWise_IA;

namespace FuelWise.ViewModels;

public partial class DataViewModel : ObservableObject
{
    private readonly IBluetoothConnector bluetoothConnector;
    private readonly IDataPuller dataPuller;
    private readonly IWiseCalculations wiseCalculations;
    private readonly IMLPredictions mlPredictions;
    private bool isGeneratingReport;

    [ObservableProperty]
    private string? report;

    [ObservableProperty]
    private string? gear = "0";

    [ObservableProperty]
    private string? rpm = "0000";

    [ObservableProperty]
    private string? speed = "000";

    public DataViewModel(IBluetoothConnector bluetoothConnector, IDataPuller dataPuller, IWiseCalculations wiseCalculations, IMLPredictions mlPredictions)
    {
        this.bluetoothConnector = bluetoothConnector;
        this.dataPuller = dataPuller;
        this.wiseCalculations = wiseCalculations;
        this.mlPredictions = mlPredictions;
    }

    private IDispatcherTimer? timer;

    [RelayCommand]
    public async Task RequestData()
    {
        if (!bluetoothConnector.IsConnected)
            return;

        if (timer is null)
        {
            timer = Application.Current?.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1 / 4);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        else
        {
            timer.Stop();
            timer = null;
        }

        try
        {
            var engineLoadData = await dataPuller.PullDataAsync<EngineLoadData>();
            var rpmData = await dataPuller.PullDataAsync<RpmData>();
            var mapData = await dataPuller.PullDataAsync<IntakeManifoldPressureData>();
            var intakeAirTempData = await dataPuller.PullDataAsync<IntakeAirTemperatureData>();
            var throttlePositionData = await dataPuller.PullDataAsync<ThrottlePositionData>();
            var speedData = await dataPuller.PullDataAsync<VehicleSpeedData>();

            var estimatedMaf = mlPredictions.PredictMAF(engineLoadData.Value, rpmData.Value, mapData.Value, intakeAirTempData.Value, throttlePositionData.Value);

            var volumetricEfficiency = wiseCalculations.GetVolumetricEfficiency(rpmData.Value, estimatedMaf);

            var imap = wiseCalculations.GetCalculatedImap(rpmData.Value, mapData.Value, intakeAirTempData.Value);
            var maf = wiseCalculations.GetCalculatedMaf(imap, volumetricEfficiency);
            var gear = wiseCalculations.GetCurrentGear(rpmData.Value, speedData.Value);

            var sb = new StringBuilder();
            sb.AppendLine($"Estimated MAF: {estimatedMaf}");
            sb.AppendLine($"Volumetric Efficiency: {volumetricEfficiency}");
            sb.AppendLine($"IMAP: {imap}");
            sb.AppendLine($"Calculated MAF: {maf}");
            sb.AppendLine($"Gear: {gear}");

            Report = sb.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private bool isPullingData = false;

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        if (isPullingData)
            return;

        try
        {
            isPullingData = true;

            var rpmData = await dataPuller.PullDataAsync<RpmData>();
            var speedData = await dataPuller.PullDataAsync<VehicleSpeedData>();

            Speed = speedData.Value.ToString();
            Rpm = rpmData.Value.ToString();
            Gear = wiseCalculations.GetCurrentGear(rpmData.Value, speedData.Value).ToString();
        }
        catch { }
        finally
        {
            isPullingData = false;
        }
    }
}