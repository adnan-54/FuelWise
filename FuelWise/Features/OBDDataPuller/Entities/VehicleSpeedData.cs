using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class VehicleSpeedData : ObdData<int>
{
    public VehicleSpeedData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Vehicle Speed";

    public override string Description => "Vehicle speed";

    public override int MaxValue => 255;

    public override int MinValue => 0;

    public override Unit Unit => Unit.KilometerPerHour;

    protected override int GetValue()
    {
        return Data.A.Value;
    }

    public float ToMeterPerSecond()
    {
        return GetValue() / 3.6F;
    }
}