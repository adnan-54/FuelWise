namespace FuelWise.OBDProtocol;

public enum PID
{
    EngineLoad = 0x04,
    EngineCoolantTemperature = 0x05,
    IntakeManifoldPressure = 0x0B,
    RPM = 0x0C,
    VehicleSpeed = 0x0D,
    IntakeAirTemperature = 0x0F,
    ThrottlePosition = 0x11,
    FuelType = 0x51,

    Unknown = 0xFF,
}