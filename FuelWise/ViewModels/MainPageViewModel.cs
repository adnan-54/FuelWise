using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions.Contracts;
using FuelWise.Services;

namespace FuelWise.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IBluetoothService bluetoothService;

    [ObservableProperty]
    private bool isSearchingForDevices;

    [ObservableProperty]
    private List<IDevice> availableDevices;

    public MainPageViewModel(IBluetoothService bluetoothService)
    {
        this.bluetoothService = bluetoothService;

        availableDevices = new();
    }

    [RelayCommand]
    public async Task FindDevices()
    {
        try
        {
            IsSearchingForDevices = true;
            var foundDevices = await bluetoothService.ScanDevices();
            AvailableDevices = new(foundDevices);
        }
        catch
        {

        }
        finally
        {
            IsSearchingForDevices = false;
        }
    }
}
