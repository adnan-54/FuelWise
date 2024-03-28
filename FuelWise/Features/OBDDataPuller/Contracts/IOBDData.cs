using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public interface IOBDData
{
    Frame Frame { get; }

    string Name { get; }

    string Description { get; }

    Unit Unit { get; }

    object Value { get; }

    object MaxValue { get; }

    object MinValue { get; }
}

public interface IOBDData<TValue> : IOBDData
{
    new TValue Value { get; }

    new TValue MaxValue { get; }

    new TValue MinValue { get; }
}