using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public abstract class ObdData<TValue> : IOBDData<TValue> where TValue : struct
{
    protected ObdData(Frame frame)
    {
        Frame = frame;
    }

    public Frame Frame { get; }

    protected Data Data => Frame.Payload.Data;

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract Unit Unit { get; }

    public TValue Value => GetValue();

    public abstract TValue MaxValue { get; }

    public abstract TValue MinValue { get; }

    protected abstract TValue GetValue();

    object IOBDData.Value => Value;

    object IOBDData.MaxValue => MaxValue;

    object IOBDData.MinValue => MinValue;

    public override string ToString()
    {
        return $"{Name}: {Unit.ToFormattedString(Value)}";
    }
}