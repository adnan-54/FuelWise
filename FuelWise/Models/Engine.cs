﻿namespace FuelWise.Models;

public record Engine(string Name, double Displacement, int Cylinders, double Power, int PowerPeakRpm, double Torque, int TorquePeakRpm);
