namespace FuelWise.Models;

public record OBD(int Speed, int RPM, int EngineCoolantTemp, int FuelLevel,
                int ThrottlePosition, int IntakeAirTemperature, int MAF,
                int TimingAdvance, int FuelPressure, int EngineLoad, int FuelTankLevelInput,
                int FuelAirCommanded, int TimeSinceTroubleCodesCleared, DateTime ReadAt);
