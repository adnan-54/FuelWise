using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class IntakeAirTemperatureData : ObdData<int>
{
    public IntakeAirTemperatureData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Intake Temperature";

    public override string Description => "Intake air temperature";

    public override int MaxValue => 215;

    public override int MinValue => -40;

    public override Unit Unit => Unit.Celsius;

    public override int GetValue()
    {
        return Data.A.Value - 40;
    }
}