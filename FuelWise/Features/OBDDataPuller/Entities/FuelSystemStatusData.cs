using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class FuelSystemStatusData : ObdData<int>
{
    public FuelSystemStatusData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Fuel System Status";

    public override string Description => "Fuel system stauts";

    public override int MaxValue => 16;

    public override int MinValue => 0;

    public override Unit Unit => Unit.None;

    protected override int GetValue()
    {
        return Data.A.Value;
    }
}