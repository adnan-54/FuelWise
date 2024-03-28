using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public sealed class EthanolFuelPercentageData : ObdData<int>
{
    public EthanolFuelPercentageData(Frame frame) : base(frame)
    {
    }

    public override string Name => "Ethanol Percentage";

    public override string Description => "Ethanol fuel percentage";

    public override int MaxValue => 100;

    public override int MinValue => 0;

    public override Unit Unit => Unit.Percent;

    public override int GetValue()
    {
        return Convert.ToInt32(Data.A.Value / 2.55);
    }
}