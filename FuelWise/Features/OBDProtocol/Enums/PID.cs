namespace FuelWise.OBDProtocol;

public enum PID
{
    EngineLoad = 0x04,
    EngineCoolantTemperature = 0x05,
    FuelPressure = 0x0A,
    IntakeManifoldPressure = 0x0B,
    EngineSpeed = 0x0C,
    VehicleSpeed = 0x0D,
    IntakeAirTemperature = 0x0F,
    MassAirFlow = 0x10,
    ThrottlePosition = 0x11,
    RunTimeSinceEngineStart = 0x1F,
    FuelLevel = 0x2F,
    FuelType = 0x51,
    EthanolFuelPercentage = 0x52,
    EngineOilTemperature = 0x5C,
    EngineFuelRate = 0x5E,
    TransmissionActualGear = 0xA4,
    Odometer = 0xA6,
}