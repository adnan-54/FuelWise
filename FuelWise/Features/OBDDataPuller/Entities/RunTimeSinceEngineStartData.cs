using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class RunTimeSinceEngineStartData : ObdData<double>
{
    public RunTimeSinceEngineStartData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Engine Run Time";

    public override string Description => "Run time since engine started";

    public override double MaxValue => 65.535;

    public override double MinValue => 0;

    public override Unit Unit => Unit.Seconds;

    public override double GetValue()
    {
        return 256 * Data.A.Value + Data.B.Value;
    }
}