namespace FuelWise.Models;

public record Wheel(double RimDiameter, double TireWidth, double TireHeigth)
{
    public double TireCircunference => Math.PI * (RimDiameter * 25.4 + ((TireWidth * TireHeigth) / 100) * 2);

    public double RollingRadius => TireCircunference / Math.PI / 2;
};


