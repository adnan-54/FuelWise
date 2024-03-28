using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class FuelPressureData : ObdData<int>
{
    public FuelPressureData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Fuel Pressure";

    public override string Description => "Fuel pressure";

    public override int MaxValue => 765;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Kpa;

    public override int GetValue()
    {
        return Data.A.Value * 3;
    }
}