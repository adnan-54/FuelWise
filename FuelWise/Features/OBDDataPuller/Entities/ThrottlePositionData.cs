using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class ThrottlePositionData : ObdData<int>
{
    public ThrottlePositionData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Throttle Position";

    public override string Description => "Throttle position";

    public override int MaxValue => 100;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Percent;

    public override int GetValue()
    {
        return Convert.ToInt32(Data.A.Value / 2.55);
    }
}