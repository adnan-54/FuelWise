using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class FuelLevelData : ObdData<int>
{
    public FuelLevelData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Fuel Tank Level";

    public override string Description => "Fuel tank level input";

    public override int MaxValue => 100;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Percent;

    public override int GetValue()
    {
        return Convert.ToInt32(Data.A.Value / 2.55);
    }
}