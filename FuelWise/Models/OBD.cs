namespace FuelWise.Models;

public record OBD(int Speed, int RPM, int CoolantTemp, int FuelLevel,
                int ThrottlePosition, int Mileage, int EngineLoad, DateTime ReadAt);
