using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public delegate void FrameReceivedEventHandler(object sender, FrameReceivedEventArgs e);

public sealed class FrameReceivedEventArgs : EventArgs
{
    public FrameReceivedEventArgs(Frame data, IOBDData request)
    {
        Data = data;
        Request = request;
    }

    public Frame Data { get; }

    public IOBDData Request { get; }
}