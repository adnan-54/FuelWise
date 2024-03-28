using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuelWise.BluetoothConnection;
using FuelWise.OBDDataPuller;

namespace FuelWise.ViewModels;

public partial class DataViewModel : ObservableObject
{
    private readonly IBluetoothConnector bluetoothConnector;
    private readonly IDataPuller dataPuller;

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
            var sb = new StringBuilder();

            var engineLoadData = await dataPuller.PullDataAsync<EngineLoadData>();
            sb.AppendLine(engineLoadData.ToString());

            var coolantData = await dataPuller.PullDataAsync<EngineCoolantTemperatureData>();
            sb.AppendLine(coolantData.ToString());

            var intakeManifoldPressureData = await dataPuller.PullDataAsync<IntakeManifoldPressureData>();
            sb.AppendLine(intakeManifoldPressureData.ToString());

            var rpmData = await dataPuller.PullDataAsync<RpmData>();
            sb.AppendLine(rpmData.ToString());

            var speedData = await dataPuller.PullDataAsync<VehicleSpeedData>();
            sb.AppendLine(speedData.ToString());

            var intakeAirTempData = await dataPuller.PullDataAsync<IntakeAirTemperatureData>();
            sb.AppendLine(intakeAirTempData.ToString());

            var throttlePositionData = await dataPuller.PullDataAsync<ThrottlePositionData>();
            sb.AppendLine(throttlePositionData.ToString());

            /*
            var fuelTypeData = await dataPuller.PullDataAsync<FuelTypeData>();
            sb.AppendLine(fuelTypeData.ToString());
            */

            Report = sb.ToString();
        }
        catch (Exception ex)
        {
        }
    }
}