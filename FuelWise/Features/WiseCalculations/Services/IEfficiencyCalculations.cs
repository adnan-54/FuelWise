using FuelWise.Reporting;

namespace FuelWise.WiseCalculations;

public enum PossibleAction
{
    ReduceThrottle,
    ReduceSpeed,
    ReduceRpm,
    DecreaseGear,
    IncreaseGear
}

public interface IEfficiencyCalculations
{
    double CalculateEfficiency(Report lastReport);

    PossibleAction GetCalculatedAction(Report lastReport);
}