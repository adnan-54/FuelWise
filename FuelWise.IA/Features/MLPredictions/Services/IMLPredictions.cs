using System;
using System.Linq;
using DrivingStyleModel = FuelWise_IA.DrivingStyle;
using FuelConsumptionModel = FuelWise_IA.FuelConsumption;
using MassAirFlowModel = FuelWise_IA.MassAirFlow;

namespace FuelWise.IA;

public interface IMLPredictions
{
    /// <summary>
    /// Tries to predict the driving style based on the given parameters.
    /// </summary>
    /// <param name="speed">Vehicle speed in km/h</param>
    /// <param name="speedAverage">Vehicle average speed in km/h for the last 60 seconds</param>
    /// <param name="speedVariation">Vehicle speed variation in km/h since the last reading</param>
    /// <param name="engineLoad">Engine load in percentage (0 - 100 range)</param>
    /// <param name="coolantTemperature">Coolant temperature in Celsius</param>
    /// <param name="intakeAirTemperature">Intake air temperature in Celsius</param>
    /// <param name="intakePressure">Intake pressure in kPa</param>
    /// <param name="massAirFlow">Mass air flow in g/s</param>
    /// <param name="rpm">Engine RPM</param>
    /// <param name="fuelConsumptionAverage">Average fuel consumption in km/L</param>
    /// <param name="roadSurface">Possible road surface conditions</param>
    /// <param name="trafficCondition">Possible traffic conditions</param>
    /// <returns>A tuple containing the enum representing the predicted driving style and a double representing the efficiency in percentage (0 - 100 range)</returns>
    (DrivingStyle, double) PredictDrivingStyle(float speed, float speedAverage, float speedVariation, float engineLoad,
                              float coolantTemperature, float intakeAirTemperature, float intakePressure,
                              float massAirFlow, float rpm, float fuelConsumptionAverage,
                              RoadSurfaceCondition roadSurface = RoadSurfaceCondition.Smooth,
                              TrafficCondition trafficCondition = TrafficCondition.LowCongestion);

    /// <summary>
    /// Tries to predict the fuel consumption based on the given parameters.
    /// </summary>
    /// <param name="speed">Vehicle speed in km/h</param>
    /// <param name="speedAverage">Vehicle average speed in km/h for the last 60 seconds</param>
    /// <param name="speedVariation">Vehicle speed variation in km/h since the last reading</param>
    /// <param name="engineLoad">Engine load in percentage (0 - 100 range)</param>
    /// <param name="coolantTemperature">Coolant temperature in Celsius</param>
    /// <param name="intakeAirTemperature">Intake air temperature in Celsius</param>
    /// <param name="intakePressure">Intake pressure in kPa</param>
    /// <param name="massAirFlow">Mass air flow in g/s</param>
    /// <param name="rpm">Engine RPM</param>
    /// <param name="drivingStyle">Last driving style recorded</param>
    /// <param name="roadSurface">Possible road surface conditions</param>
    /// <param name="trafficCondition">Possible traffic conditions</param>
    /// <returns>A double representing the predicted fuel consumption in km/L</returns>
    double PredictFuelComsumption(float speed, float speedAverage, float speedVariation, float engineLoad,
                                  float coolantTemperature, float intakeAirTemperature, float intakePressure,
                                  float massAirFlow, float rpm, DrivingStyle drivingStyle,
                                  RoadSurfaceCondition roadSurface = RoadSurfaceCondition.Smooth,
                                  TrafficCondition trafficCondition = TrafficCondition.LowCongestion);

    /// <summary>
    /// Tries to predict the mass air flow based on the given parameters.
    /// </summary>
    /// <param name="engineLoad">Engine load in percentage (0 - 100 range)</param>
    /// <param name="rpm">Engine RPM</param>
    /// <param name="intakePressure">Intake pressure in kPa</param>
    /// <param name="intakeAirTemperature">Intake air temperature in Celsius</param>
    /// <param name="throttlePosition">Throttle position in percentage (0 - 100 range)</param>
    /// <returns>A double representing the predicted mass air flow in g/s</returns>
    double PredictMAF(float engineLoad, float rpm, float intakePressure, float intakeAirTemperature, float throttlePosition);
}

internal class MLPredictions : IMLPredictions
{
    public (DrivingStyle, double) PredictDrivingStyle(float speed, float speedAverage, float speedVariation, float engineLoad,
                                     float coolantTemperature, float intakeAirTemperature, float intakePressure,
                                     float massAirFlow, float rpm, float fuelConsumptionAverage,
                                     RoadSurfaceCondition roadSurface = RoadSurfaceCondition.Smooth,
                                     TrafficCondition trafficCondition = TrafficCondition.LowCongestion)
    {
        if (rpm == 0)
            return (DrivingStyle.Even, 100);

        //converting from km/L to L/100km
        fuelConsumptionAverage = 100 / fuelConsumptionAverage;

        var input = new DrivingStyleModel.ModelInput
        {
            VehicleSpeedInstantaneous = speed,
            VehicleSpeedAverage = speedAverage,
            VehicleSpeedVariation = speedVariation,
            EngineLoad = engineLoad,
            EngineCoolantTemperature = coolantTemperature,
            IntakeAirTemperature = intakeAirTemperature,
            ManifoldAbsolutePressure = intakePressure,
            MassAirFlow = massAirFlow,
            EngineRPM = rpm,
            FuelConsumptionAverage = fuelConsumptionAverage,
            Traffic = (int)trafficCondition,
            RoadSurface = (int)roadSurface,
        };

        var result = DrivingStyleModel.PredictAllLabels(input);

        var drivingStyle = result.First().Key == "1" ? DrivingStyle.Even : DrivingStyle.Aggresive;

        var efficiency = drivingStyle == DrivingStyle.Even ? result.First().Value : 1 - result.First().Value;
        efficiency *= 100;

        return (drivingStyle, efficiency);
    }

    public double PredictFuelComsumption(float speed, float speedAverage, float speedVariation, float engineLoad,
                                         float coolantTemperature, float intakeAirTemperature, float intakePressure,
                                         float massAirFlow, float rpm, DrivingStyle drivingStyle,
                                         RoadSurfaceCondition roadSurface = RoadSurfaceCondition.Smooth,
                                         TrafficCondition trafficCondition = TrafficCondition.LowCongestion)
    {
        if (rpm == 0)
            return 0;

        var input = new FuelConsumptionModel.ModelInput
        {
            VehicleSpeedInstantaneous = speed,
            VehicleSpeedAverage = speedAverage,
            VehicleSpeedVariation = speedVariation,
            EngineLoad = engineLoad,
            EngineCoolantTemperature = coolantTemperature,
            IntakeAirTemperature = intakeAirTemperature,
            ManifoldAbsolutePressure = intakePressure,
            MassAirFlow = massAirFlow,
            EngineRPM = rpm,
            Traffic = (int)trafficCondition,
            RoadSurface = (int)roadSurface,
            DrivingStyle = (int)drivingStyle
        };

        var result = FuelConsumptionModel.Predict(input);
        var l100km = result.Score;

        return 100 / l100km;
    }

    public double PredictMAF(float engineLoad, float rpm, float intakePressure, float intakeAirTemperature, float throttlePosition)
    {
        if (rpm == 0)
            return 0;

        var input = new MassAirFlowModel.ModelInput
        {
            EngineLoad = engineLoad,
            RPM = rpm,
            IntakeManifoldPressure = intakePressure,
            IntakeAirTemperature = intakeAirTemperature,
            ThrottlePosition = throttlePosition
        };

        var result = MassAirFlowModel.Predict(input);

        return result.Score;
    }
}