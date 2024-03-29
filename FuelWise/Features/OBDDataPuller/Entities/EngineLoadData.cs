using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class EngineLoadData : ObdData<int>
{
    public EngineLoadData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Engine Load";

    public override string Description => "Calculated engine load";

    public override int MaxValue => 100;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Percent;

    protected override int GetValue()
    {
        return Convert.ToInt32(Data.A.Value / 2.55);
    }
}