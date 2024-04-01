namespace FuelWise.VehicleInformations;

public delegate void VehicleChangedEventHandler(object sender, VehicleChangedArgs e);

public class VehicleChangedArgs
{
    public VehicleChangedArgs(Vehicle? vehicle)
    {
        Vehicle = vehicle;
    }

    public Vehicle? Vehicle { get; }
}