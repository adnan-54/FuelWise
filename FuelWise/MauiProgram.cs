using CommunityToolkit.Maui;
using FuelWise.BluetoothConnection;
using FuelWise.IA;
using FuelWise.NativeDialog;
using FuelWise.OBDDataPuller;
using FuelWise.OBDEncoder;
using FuelWise.Reporting;
using FuelWise.VehicleInformations;
using FuelWise.ViewModels;
using FuelWise.WiseCalculations;
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

        builder.Services.AddMachineLearning();

        builder.Services.AddSingleton<IDialogManager, DefaultDialogManager>();
        builder.Services.AddSingleton<IOBDEncoder, DefaultOBDEncoder>();
        builder.Services.AddSingleton<IDataPuller, DefaultDataPuller>();
        builder.Services.AddSingleton<IDataFactory, DefaultDataFactory>();
        builder.Services.AddSingleton<IRequestProcessor, DefaultRequestProcessor>();
        builder.Services.AddSingleton<IVehicleProvider, DefaultVehicleProvider>();
        builder.Services.AddSingleton<IVehicleRepository, DefaultVehicleRepository>();
        builder.Services.AddSingleton<IWiseCalculations, DefaultWiseCalculations>();
        builder.Services.AddSingleton<IReportGenerator, DefaultReportGenerator>();

        builder.Services.AddTransient<MainPage>();

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainViewViewModel>();
        builder.Services.AddTransient<DataViewModel>();
        builder.Services.AddTransient<ConnectionViewModel>();

        return builder.Build();
    }
}