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
    private const double REFRESH_RATE = 0.5;

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
        var lastSecondsReports = reports.Where(r => r.CreatedAt > DateTime.Now.AddSeconds(-10)).ToList();
        var lastMinuteReports = reports.Where(r => r.CreatedAt > DateTime.Now.AddMinutes(-1)).ToList();

        var coolantData = dataPuller.PullDataAsync<EngineCoolantTemperatureData>();
        var engineLoadData = dataPuller.PullDataAsync<EngineLoadData>();
        var intakeTemperatureData = dataPuller.PullDataAsync<IntakeAirTemperatureData>();
        var intakePressureData = dataPuller.PullDataAsync<IntakeManifoldPressureData>();
        var rpmData = dataPuller.PullDataAsync<RpmData>();
        var throttlePositionData = dataPuller.PullDataAsync<ThrottlePositionData>();
        var speedData = dataPuller.PullDataAsync<VehicleSpeedData>();
        var fuelTrimData = dataPuller.PullDataAsync<ShortTermFuelTrimData>();
        var timingAdvanceData = dataPuller.PullDataAsync<TimingAdvanceData>();

        await Task.WhenAll(coolantData, engineLoadData, intakeTemperatureData, intakePressureData, rpmData, throttlePositionData, speedData, fuelTrimData, timingAdvanceData);

        var speed = speedData.Result.Value;
        var averageSpeed = wiseCalculations.GetAverageSpeed(lastMinuteReports.Select(r => r.Speed));
        var speedVariation = speed - lastReport.Speed;
        var rpm = rpmData.Result.Value;
        var coolantTemperature = coolantData.Result.Value;
        var engineLoad = engineLoadData.Result.Value;
        var intakeAirTemperature = intakeTemperatureData.Result.Value;
        var intakePressure = intakePressureData.Result.Value;
        var throttlePosition = throttlePositionData.Result.Value;
        var fuelTrim = fuelTrimData.Result.Value;
        var timingAdvance = timingAdvanceData.Result.Value;

        var gear = wiseCalculations.GetCurrentGear(rpm, speed);

        var predictedMaf = mlPredictions.PredictMAF(engineLoad, rpm, intakePressure, intakeAirTemperature, throttlePosition, coolantTemperature, (float)fuelTrim, speed, (float)timingAdvance);
        predictedMaf = (predictedMaf + lastReport.MassAirFlow) / 2;
        var volumetricEfficiency = wiseCalculations.GetVolumetricEfficiency(rpm, predictedMaf, intakeAirTemperature, intakePressure);
        var imap = wiseCalculations.GetImap(rpm, intakePressure, intakeAirTemperature);
        var calculatedMaf = wiseCalculations.GetCalculatedMaf(imap, volumetricEfficiency);
        var maf = (predictedMaf + calculatedMaf) / 2;

        var isOnHighway = averageSpeed > 60;

        var predictedFuelComsumption = mlPredictions.PredictFuelComsumption(speed, (float)averageSpeed, (float)speedVariation, engineLoad, coolantTemperature, intakeAirTemperature, intakePressure, (float)maf, rpm, lastReport.DrivingStyle);
        var fuelComsumption = wiseCalculations.GetFuelComsumption(predictedFuelComsumption, speed, maf, throttlePosition, rpm);
        var averageFuelComsumption = wiseCalculations.GetAverageFuelComsumption(lastSecondsReports.Select(r => r.FuelConsumption));
        var consumptionVariance = wiseCalculations.GetComsumptionVariance(lastSecondsReports.Select(r => r.FuelConsumption));
        var fuelEfficiency = wiseCalculations.GetFuelEfficiency(fuelComsumption, isOnHighway);

        var drivingStyle = consumptionVariance > 33 ? DrivingStyle.Aggressive : DrivingStyle.Even;
        var averageDrivingStyle = reports.TakeLast(100).Where(r => r.DrivingStyle == DrivingStyle.Even).Count();

        var drivingEfficiency = (fuelEfficiency + averageDrivingStyle) / 2;
        var averageDrivingEfficiency = lastSecondsReports.Any() ? lastSecondsReports.Average(r => r.DrivingEfficiency) : 0;

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

            DrivingStyle = drivingStyle,

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
        timer.Interval = TimeSpan.FromMilliseconds(100);
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