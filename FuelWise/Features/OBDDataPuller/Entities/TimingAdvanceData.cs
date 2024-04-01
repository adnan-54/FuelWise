using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class TimingAdvanceData : ObdData<double>
{
    public TimingAdvanceData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Timing Advance";

    public override string Description => "Timing advance";

    public override double MaxValue => 63.5;

    public override double MinValue => -64;

    public override Unit Unit => Unit.Degrees;

    protected override double GetValue()
    {
        return (Data.A.Value / 2) - 64;
    }
}