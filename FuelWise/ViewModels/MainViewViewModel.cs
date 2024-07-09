using CommunityToolkit.Mvvm.ComponentModel;
using FuelWise.DriverFeedback;
using FuelWise.Reporting;
using FuelWise.VehicleInformations;

namespace FuelWise.ViewModels;

public partial class MainViewViewModel : ObservableObject
{
    private readonly IReportGenerator reportGenerator;
    private readonly IVehicleProvider vehicleProvider;

    [ObservableProperty]
    private string? vehicleName;

    [ObservableProperty]
    private int speed;

    [ObservableProperty]
    private double averageComsumption;

    [ObservableProperty]
    private int averageEfficiency;

    [ObservableProperty]
    private bool isCalibrating;

    [ObservableProperty]
    private double calibrationProgress;

    [ObservableProperty]
    private bool isUnderOperatingTemperature;

    [ObservableProperty]
    private bool isOverheating;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowAction))]
    private CurrentFeedback feedback = CurrentFeedback.None;

    public MainViewViewModel(IReportGenerator reportGenerator,
        IVehicleProvider vehicleProvider,
        IDriverFeedback driverFeedback)
    {
        this.reportGenerator = reportGenerator;
        this.vehicleProvider = vehicleProvider;

        vehicleProvider.VehicleChanged += OnVehicleChanged;
        reportGenerator.ReportGenerated += OnReportGenerated;
        driverFeedback.FeedbackReceived += OnFeedbackReceived;
    }

    public bool ShowAction => Feedback != CurrentFeedback.None;

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

        var engine = vehicleProvider.Vehicle?.Engine;
        var operatingTemperature = engine?.OperatingTemperature ?? 0;
        var coolantTemperature = e.Report.CoolantTemperature;

        IsUnderOperatingTemperature = coolantTemperature < operatingTemperature * 0.9;
        IsOverheating = coolantTemperature > operatingTemperature + 9;

        IsCalibrating = reportGenerator.Reports.Count < 100;

        if (IsCalibrating)
            CalibrationProgress = reportGenerator.Reports.Count;
    }

    private void OnFeedbackReceived(object sender, FeedbackReceivedEventArgs e)
    {
        Feedback = e.Feedback;
    }
}