using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class EngineCoolantTemperatureData : ObdData<int>
{
    public EngineCoolantTemperatureData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Coolant Temperature";

    public override string Description => "Engine coolant temperature";

    public override int MaxValue => 215;

    public override int MinValue => -40;

    public override Unit Unit => Unit.Celsius;

    protected override int GetValue()
    {
        return Data.A.Value - 40;
    }
}