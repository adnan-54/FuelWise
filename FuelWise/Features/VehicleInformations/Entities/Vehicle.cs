namespace FuelWise.VehicleInformations;

public record Vehicle(string Maker, string Name, string VIN, string Color, int ModelYear, int Weigth, Engine Engine, Transmission Transmission, Tyre Tyre);