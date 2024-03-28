using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class EngineFuelRateData : ObdData<double>
{
    public EngineFuelRateData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Engine Fuel Rate";

    public override string Description => "Engine fuel rate";

    public override double MaxValue => 3212.75;

    public override double MinValue => 0;

    public override Unit Unit => Unit.LitersPerHour;

    public override double GetValue()
    {
        return ((256 * Data.A.Value) + Data.B.Value) / 20;
    }
}