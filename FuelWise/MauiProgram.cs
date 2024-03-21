using CommunityToolkit.Maui;
using FuelWise.Services;
using FuelWise.ViewModels;
using Microsoft.Extensions.Logging;

namespace FuelWise;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if ANDROID
        builder.Services.AddSingleton<IBluetoothConnector, Platforms.Android.AndroidBluetoothConnector>();
#endif

        builder.Services.AddSingleton<IDialogManager, DefaultDialogManager>();

        builder.Services.AddSingleton<IConsumptionCalculator, ConsumptionCalculator>();
        builder.Services.AddSingleton<IVehicleProvider, VehicleProvider>();
        builder.Services.AddSingleton<IGearController, GearController>();
        builder.Services.AddSingleton<IEfficiencyCalculator, EfficiencyCalculator>();

        builder.Services.AddTransient<MainPage>();

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainViewViewModel>();
        builder.Services.AddTransient<DataViewModel>();
        builder.Services.AddTransient<ConnectionViewModel>();

        return builder.Build();
    }
}
