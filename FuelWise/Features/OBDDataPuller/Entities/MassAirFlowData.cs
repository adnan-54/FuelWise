using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class MassAirFlowData : ObdData<double>
{
    public MassAirFlowData(Frame frame) : base(frame)
    {
    }

    public override string Name => "MAF";

    public override string Description => "Mass air flow sensor (MAF) air flow rate";

    public override double MaxValue => 655.35;

    public override double MinValue => 0;

    public override Unit Unit => Unit.GramsPerSecond;

    public override double GetValue()
    {
        return ((256 * Data.A.Value) + Data.B.Value) / 100;
    }
}