using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class RpmData : ObdData<double>
{
    public RpmData(Frame frame) : base(frame)
    {
    }

    public override string Name => "RPM";

    public override string Description => "Engine RPM";

    public override double MaxValue => 16383.75;

    public override double MinValue => 0;

    public override Unit Unit => Unit.Rpm;

    public override double GetValue()
    {
        return ((Data.A.Value * 256) + Data.B.Value) / 4.0;
    }
}