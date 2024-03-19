using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuelWise.Services;

namespace FuelWise.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    private readonly IBluetoothService bluetoothService;

    [ObservableProperty]
    private bool isSearchingForDevices;

    [ObservableProperty]
    private List<string>? availableDevices;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanShowDeviceInfos))]
    private string? selectedDevice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoDevicesToShow))]
    private bool hasDevicesToShow;

    public ConnectionViewModel(IBluetoothService bluetoothService)
    {
        this.bluetoothService = bluetoothService;
        availableDevices = new();
        hasDevicesToShow = false;
    }

    public bool CanShowDeviceInfos => SelectedDevice is not null;

    public bool HasNoDevicesToShow => !HasDevicesToShow;

    [RelayCommand]
    public async Task FindDevices()
    {
        try
        {
            IsSearchingForDevices = true;
            var foundDevices = await bluetoothService.ScanDevices();

            // Link de exemplo para configurar as permissões: https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le/blob/master/Source/BLE.Client/BLE.Client.Maui/Platforms/Android/DroidPlatformHelpers.cs
            PermissionStatus status;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                status = await Permissions.CheckStatusAsync<BluetoothPermissions>();

                if (status != PermissionStatus.Granted)
                    await Application.Current.MainPage.DisplayAlert("Permission required", "Bluetooth scanning.", "OK");


                status = await Permissions.RequestAsync<BluetoothPermissions>();
            }
            else
            {
                status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                    await Application.Current.MainPage.DisplayAlert("Permission required", "Location permission is required for bluetooth scanning. We do not store or use your location at all.", "OK");

                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                    await Application.Current.MainPage.DisplayAlert("Permission required", "Location permission is required for bluetooth scanning. We do not store or use your location at all.", "OK");

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            //Gets a list of all connected Bluetooth devices
            var ConnectedDevices = _conditional.GetConnectedDevices();
            // Como fazer o bind dos valores list<string>: https://stackoverflow.com/questions/2765369/binding-to-an-observablecollectionstring-listview?rq=4

            AvailableDevices = new(ConnectedDevices);
            HasDevicesToShow = AvailableDevices.Any();
        }
        catch
        {

        }
        finally
        {
            IsSearchingForDevices = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowDeviceInfos))]
    public async Task ShowDeviceInfos()
    {
        if (SelectedDevice is null)
            return;

        var propertyInfos = typeof(IDevice).GetProperties(BindingFlags.Public);

        foreach (var propertyInfo in propertyInfos)
        {
            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(SelectedDevice);

            var result = await Application.Current!.MainPage!.DisplayAlert("Device Infos", $"{propertyName}: {propertyValue}", "Next", "Exit");

            if (!result)
                break;
        }
    }
}
