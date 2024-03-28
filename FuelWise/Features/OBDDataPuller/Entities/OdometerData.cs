using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class OdometerData : ObdData<double>
{
    public OdometerData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Odometro";

    public override string Description => "Odometro";

    public override double MaxValue => 429496729.5;

    public override double MinValue => 0;

    public override Unit Unit => Unit.Kilometer;

    public override double GetValue()
    {
        return (Data.A.Value * (2 ^ 24)) + (Data.B.Value * (2 ^ 16)) + (Data.C.Value * (2 ^ 8)) + Data.D.Value;
    }
}