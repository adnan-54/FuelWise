using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class IntakeManifoldPressureData : ObdData<int>
{
    public IntakeManifoldPressureData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Intake Pressure";

    public override string Description => "Intake manifold absolute pressure";

    public override int MaxValue => 255;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Kpa;

    public override int GetValue()
    {
        return Data.A.Value;
    }
}