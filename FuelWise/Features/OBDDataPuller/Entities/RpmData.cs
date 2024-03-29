using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class RpmData : ObdData<int>
{
    public RpmData(Frame frame) : base(frame)
    {
    }

    public override string Name => "RPM";

    public override string Description => "Engine RPM";

    public override int MaxValue => 16383;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Rpm;

    protected override int GetValue()
    {
        var rpm = ((Data.A.Value * 256) + Data.B.Value) / 4.0;

        return Convert.ToInt32(rpm);
    }
}