using FuelWise.Models;

namespace FuelWise.Events;

public class DataPulledEventArgs : EventArgs
{
    public DataPulledEventArgs(OBD obd)
    {
        Obd = obd;
    }

    public OBD Obd { get; }
}
