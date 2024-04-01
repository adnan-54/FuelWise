namespace FuelWise.OBDProtocol;

public enum PID
{
    EngineLoad = 0x04,
    EngineCoolantTemperature = 0x05,
    ShortTermFuelTrim = 0x06,
    IntakeManifoldPressure = 0x0B,
    RPM = 0x0C,
    VehicleSpeed = 0x0D,
    TimingAdvance = 0x0E,
    IntakeAirTemperature = 0x0F,
    ThrottlePosition = 0x11,
    FuelType = 0x51,

    Unknown = 0xFF,
}