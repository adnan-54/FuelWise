using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class TransmissionActualGearData : ObdData<double>
{
    public TransmissionActualGearData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Engine Oil Temperature";

    public override string Description => "Engine oil temperature";

    public override double MaxValue => 65.535;

    public override double MinValue => 0;

    public override Unit Unit => Unit.Ratio;

    public override double GetValue()
    {
        var supported = Data.A.Value == 1;

        if (!supported)
            return 0;

        return ((256 * Data.C.Value) + Data.D.Value) / 1000;
    }
}