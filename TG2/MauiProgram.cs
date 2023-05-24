using Microsoft.Extensions.Logging;
using Plugin.BLE;
using TG2.Services;
using TG2.ViewModels;

namespace TG2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(CrossBluetoothLE.Current);
        builder.Services.AddSingleton(CrossBluetoothLE.Current.Adapter);

        builder.Services.AddSingleton<IConsumptionCalculator, ConsumptionCalculator>();
        builder.Services.AddSingleton<IVehicleProvider, VehicleProvider>();
        builder.Services.AddSingleton<IGearController, GearController>();
        builder.Services.AddSingleton<IEfficiencyCalculator, EfficiencyCalculator>();
        builder.Services.AddSingleton<IBluetoothService, BluetoothService>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainPageViewModel>();

        return builder.Build();
    }
}
