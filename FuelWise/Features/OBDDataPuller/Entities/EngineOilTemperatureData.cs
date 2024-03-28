using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class EngineOilTemperatureData : ObdData<int>
{
    public EngineOilTemperatureData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Engine Oil Temperature";

    public override string Description => "Engine oil temperature";

    public override int MaxValue => 210;

    public override int MinValue => -40;

    public override Unit Unit => Unit.Celsius;

    public override int GetValue()
    {
        return Data.A.Value - 40;
    }
}