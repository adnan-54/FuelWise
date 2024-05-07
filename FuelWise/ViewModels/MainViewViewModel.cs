using CommunityToolkit.Mvvm.ComponentModel;
using FuelWise.Reporting;
using FuelWise.VehicleInformations;

namespace FuelWise.ViewModels;

public partial class MainViewViewModel : ObservableObject
{
    [ObservableProperty]
    private string? vehicleName;

    [ObservableProperty]
    private int speed;

    [ObservableProperty]
    private double averageComsumption;

    [ObservableProperty]
    private int averageEfficiency;

    public MainViewViewModel(IReportGenerator reportGenerator, IVehicleProvider vehicleProvider)
    {
        vehicleProvider.VehicleChanged += OnVehicleChanged;
        reportGenerator.ReportGenerated += OnReportGenerated;
    }

    private void OnVehicleChanged(object sender, VehicleChangedArgs e)
    {
        var vehicle = e.Vehicle;
        if (vehicle is null)
        {
            VehicleName = string.Empty;
            return;
        }

        VehicleName = $"{vehicle.Maker} {vehicle.Name} {vehicle.ModelYear}";
    }

    private void OnReportGenerated(object sender, ReportGeneratedEventArgs e)
    {
        Speed = Convert.ToInt32(e.Report.Speed);
        AverageComsumption = e.Report.AverageFuelConsumption;
        AverageEfficiency = Convert.ToInt32(e.Report.AverageDrivingEfficiency);
    }
}