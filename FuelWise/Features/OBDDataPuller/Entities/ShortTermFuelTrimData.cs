using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class ShortTermFuelTrimData : ObdData<double>
{
    public ShortTermFuelTrimData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Short Term Fuel Trim";

    public override string Description => "Short term fuel trim (STFT)";

    public override double MaxValue => 99.2;

    public override double MinValue => -100;

    public override Unit Unit => Unit.Percent;

    protected override double GetValue()
    {
        return (Data.A.Value / 1.28) - 100;
    }
}