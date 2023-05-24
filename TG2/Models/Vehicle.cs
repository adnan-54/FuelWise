namespace TG2.Models;

public record Vehicle(string Maker, string Model, string Version, string VIN, int FabricationYear, double FuelCapacity, double Weight, Engine Engine, Transmission Transmission, Wheel Wheel, double Autonomy);
