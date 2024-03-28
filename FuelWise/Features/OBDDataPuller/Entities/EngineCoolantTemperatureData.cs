using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class EngineCoolantTemperatureData : ObdData<double>
{
    public EngineCoolantTemperatureData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Coolant Temperature";

    public override string Description => "Engine coolant temperature";

    public override double MaxValue => 215;

    public override double MinValue => -40;

    public override Unit Unit => Unit.Celsius;

    public override double GetValue()
    {
        return Data.A.Value - 40;
    }
}