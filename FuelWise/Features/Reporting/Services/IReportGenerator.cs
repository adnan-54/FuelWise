﻿using System.Diagnostics;
using FuelWise.BluetoothConnection;
using FuelWise.IA;
using FuelWise.OBDDataPuller;
using FuelWise.VehicleInformations;
using FuelWise.WiseCalculations;

namespace FuelWise.Reporting;

public interface IReportGenerator
{
    event ReportGeneratedEventHandler? ReportGenerated;

    IReadOnlyCollection<Report> Reports { get; }
}

internal class DefaultReportGenerator : IReportGenerator
{
    private const double MPG_TO_KML = 2.352;
    private const double REFRESH_RATE = 4;

    private readonly List<Report> reports;
    private readonly IDataPuller dataPuller;
    private readonly IWiseCalculations wiseCalculations;
    private readonly IMLPredictions mlPredictions;
    private readonly IVehicleProvider vehicleProvider;

    private IDispatcherTimer? timer;
    private bool isGeneratingReport;

    public DefaultReportGenerator(IBluetoothConnector bluetoothConnector, IDataPuller dataPuller, IWiseCalculations wiseCalculations, IMLPredictions mlPredictions, IVehicleProvider vehicleProvider)
    {
        reports = [];

        bluetoothConnector.DeviceConnected += OnDeviceConnected;
        bluetoothConnector.DeviceDisconnected += OnDeviceDisconnected;
        this.dataPuller = dataPuller;
        this.wiseCalculations = wiseCalculations;
        this.mlPredictions = mlPredictions;
        this.vehicleProvider = vehicleProvider;
    }

    public event ReportGeneratedEventHandler? ReportGenerated;

    public IReadOnlyCollection<Report> Reports => reports;

    private async Task<Report> GenerateReport()
    {
        if (reports.Count == 0)
            return await GenerateFirstReport();

        var lastReport = reports.Last();
        var lastSecondsReports = reports.Where(r => r.CreatedAt > DateTime.Now.AddSeconds(-6));
        var lastMinuteReports = reports.Where(r => r.CreatedAt > DateTime.Now.AddMinutes(-1));

        var coolantData = await dataPuller.PullDataAsync<EngineCoolantTemperatureData>();
        var engineLoadData = await dataPuller.PullDataAsync<EngineLoadData>();
        var intakeTemperatureData = await dataPuller.PullDataAsync<IntakeAirTemperatureData>();
        var intakePressureData = await dataPuller.PullDataAsync<IntakeManifoldPressureData>();
        var rpmData = await dataPuller.PullDataAsync<RpmData>();
        var throttlePositionData = await dataPuller.PullDataAsync<ThrottlePositionData>();
        var speedData = await dataPuller.PullDataAsync<VehicleSpeedData>();
        var fuelTrimData = await dataPuller.PullDataAsync<ShortTermFuelTrimData>();
        var timingAdvanceData = await dataPuller.PullDataAsync<TimingAdvanceData>();

        var speed = speedData.Value;
        var averageSpeed = lastMinuteReports.Average(r => r.Speed);
        var speedVariation = speedData.Value - lastReport.Speed;
        var rpm = rpmData.Value;
        var coolantTemperature = coolantData.Value;
        var engineLoad = engineLoadData.Value;
        var intakeAirTemperature = intakeTemperatureData.Value;
        var intakePressure = intakePressureData.Value;
        var throttlePosition = throttlePositionData.Value;
        var fuelTrim = fuelTrimData.Value;
        var timingAdvance = timingAdvanceData.Value;

        var gear = wiseCalculations.GetCurrentGear(rpmData.Value, speedData.Value);

        var predictedMassAirFlow = mlPredictions.PredictMAF(engineLoad, rpm, intakePressure, intakeAirTemperature, throttlePosition, coolantTemperature, (float)fuelTrim, speed, (float)timingAdvance);
        var volumetricEfficiency = wiseCalculations.GetVolumetricEfficiency(rpm, predictedMassAirFlow, intakeAirTemperature, intakePressure) * 0.8;

        var imap = wiseCalculations.GetCalculatedImap(rpm, intakePressure, intakeAirTemperature);
        var calculatedMassAirFlow = wiseCalculations.GetCalculatedMaf(imap, volumetricEfficiency);
        var maf = (predictedMassAirFlow + calculatedMassAirFlow) / 2;

        var predictedFuelComsumption = mlPredictions.PredictFuelComsumption(speed, (float)averageSpeed, (float)speedVariation, engineLoad, coolantTemperature, intakeAirTemperature, intakePressure, (float)maf, rpm, lastReport.DrivingStyle);
        var fuelComsumption = predictedFuelComsumption;

        if (speed > 0)
        {
            var airFuelRatio = vehicleProvider.Vehicle?.Engine.GetAirFuelRatio() ?? 12.0;
            var gramsOfFuel = maf / airFuelRatio;
            var lbsOfFuel = gramsOfFuel / 453.592;
            var galsOfFuel = lbsOfFuel / 6.701;
            var galsPerHour = galsOfFuel * 3600;
            var milesPerHour = speed / 1.6;
            var milesPerGal = galsPerHour == 0 ? 0 : milesPerHour / galsPerHour;

            var calculatedFuelComsumption = milesPerGal / MPG_TO_KML;

            fuelComsumption = (predictedFuelComsumption + calculatedFuelComsumption) / 2;
        }

        var averageFuelComsumption = lastMinuteReports.Average(r => r.FuelConsumption);

        var predictedDrivingStyle = mlPredictions.PredictDrivingStyle(speed, (float)averageSpeed, (float)speedVariation, engineLoad, coolantTemperature, intakeAirTemperature, intakePressure, (float)maf, rpm, (float)averageFuelComsumption);
        var drivingEfficiency = reports.TakeLast(100).Where(r => r.DrivingStyle == DrivingStyle.Even).Count();
        var averageDrivingEfficiency = lastSecondsReports.Average(r => r.DrivingEfficiency);

        var createdAt = DateTime.Now;

        return new Report
        {
            CreatedAt = createdAt,

            Speed = speed,
            AverageSpeed = averageSpeed,
            SpeedVariation = speedVariation,

            Rpm = rpm,

            CoolantTemperature = coolantTemperature,
            EngineLoad = engineLoad,
            IntakeAirTemperature = intakeAirTemperature,
            IntakePressure = intakePressure,
            ThrottlePosition = throttlePosition,

            Gear = gear,

            FuelConsumption = fuelComsumption,
            AverageFuelConsumption = averageFuelComsumption,

            MassAirFlow = maf,

            VolumetricEfficiency = volumetricEfficiency,

            DrivingStyle = predictedDrivingStyle.DrivingStyle,

            DrivingEfficiency = drivingEfficiency,
            AverageDrivingEfficiency = averageDrivingEfficiency
        };
    }

    private static Task<Report> GenerateFirstReport()
    {
        var report = new Report
        {
            CreatedAt = DateTime.Now,

            Speed = 0,
            AverageSpeed = 0,
            SpeedVariation = 0,

            Rpm = 0,

            CoolantTemperature = 0,
            EngineLoad = 0,
            IntakeAirTemperature = 0,
            IntakePressure = 0,
            ThrottlePosition = 0,

            Gear = 0,

            FuelConsumption = 0,
            AverageFuelConsumption = 0,

            MassAirFlow = 0,

            VolumetricEfficiency = 0,

            DrivingStyle = DrivingStyle.Even,

            DrivingEfficiency = 0,
            AverageDrivingEfficiency = 0
        };

        return Task.FromResult(report);
    }

    private void OnDeviceConnected(object? sender, EventArgs e)
    {
        if (timer is not null)
        {
            timer.Stop();
            timer.Tick -= OnTick;
        }

        timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1 / REFRESH_RATE);
        timer.Tick += OnTick;
        timer.Start();
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        if (timer is not null)
        {
            timer.Stop();
            timer.Tick -= OnTick;
            timer = null;
        }
    }

    private async void OnTick(object? sender, EventArgs e)
    {
        if (isGeneratingReport)
            return;

        isGeneratingReport = true;

        try
        {
            var report = await GenerateReport();
            reports.Add(report);
            ReportGenerated?.Invoke(this, new ReportGeneratedEventArgs(report));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            isGeneratingReport = false;
        }
    }
}