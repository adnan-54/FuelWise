using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class FuelTypeData : ObdData<int>
{
    public FuelTypeData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Fuel Tank Level";

    public override string Description => "Fuel tank level input";

    public override int MaxValue => 23;

    public override int MinValue => 0;

    public override Unit Unit => Unit.None;

    protected override int GetValue()
    {
        return Data.A.Value;
    }
}