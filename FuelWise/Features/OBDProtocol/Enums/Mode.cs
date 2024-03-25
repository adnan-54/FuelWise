namespace FuelWise.OBDProtocol;

public enum Mode
{
    ShowCurrentData = 0x01,
    ShowFreezeFrameData = 0x02,
    ShowStoredDiagnosticTroubleCodes = 0x03,
    ClearDiagnosticTroubleCodesAndStoredValues = 0x04,
    RequestVehicleInformation = 0x09,
    PermanentDiagnosticTroubleCodes = 0x0A,
}