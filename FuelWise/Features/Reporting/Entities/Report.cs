using FuelWise.IA;

namespace FuelWise.Reporting;

public record Report()
{
    public required DateTime CreatedAt { get; init; }

    public required double Speed { get; init; }

    public required double AverageSpeed { get; init; }

    public required double SpeedVariation { get; init; }

    public required double Rpm { get; init; }

    public required double CoolantTemperature { get; init; }

    public required double EngineLoad { get; init; }

    public required double IntakeAirTemperature { get; init; }

    public required double IntakePressure { get; init; }

    public required double ThrottlePosition { get; init; }

    public required double FuelTrim { get; init; }

    public required int FuelStatus { get; init; }

    public required double MassAirFlow { get; init; }

    public required double VolumetricEfficiency { get; init; }

    public required int Gear { get; init; }

    public required double FuelConsumption { get; init; }

    public required double AverageFuelConsumption { get; init; }

    public required DrivingStyle DrivingStyle { get; init; }

    public required double DrivingEfficiency { get; init; }

    public required double AverageDrivingEfficiency { get; init; }

    public bool IsOnHighway => AverageSpeed > 60;

    public bool IsVehicleMoving => Speed > 0;

    public bool IsEngineRunning => Rpm > 0;
}