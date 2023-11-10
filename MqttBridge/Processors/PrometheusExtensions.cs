using MqttBridge.Models.Data.Sensor;

namespace MqttBridge.Processors;

public static class PrometheusExtensions
{
    public static string ToPrometheus(this MeasurementType type)
    {
        switch (type)
        {
            case MeasurementType.Temperatur:
                return "temperature";
            case MeasurementType.Humidity:
                return "humidity";
            case MeasurementType.Moisture:
                return "moisture";
            case MeasurementType.Pressure:
                return "pressure";
            case MeasurementType.Distance:
                return "distance";
            case MeasurementType.Luminosity:
                return "luminosity";
            case MeasurementType.LuminosityBroadband:
                return "luminosity_broadband";
            case MeasurementType.LuminosityIr:
                return "luminosity_ir";
            case MeasurementType.Co2:
                return "co2";
            case MeasurementType.Voc:
                return "voc";
            case MeasurementType.GasResistance:
                return "gas_resistance";
            case MeasurementType.Battery:
                return "battery";
            case MeasurementType.MoistureRaw:
                return "moisture_raw";
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static string ToPrometheus(this Units unit)
    {
        switch (unit)
        {
            case Units.DegreesCelsius:
                return "celsius";
            case Units.Meter:
                return "meters";
            case Units.Volt:
                return "volts";
            case Units.Percent:
                return "percent";
            case Units.HectoPascal:
                return "hectopascals";
            case Units.Lux:
                return "lux";
            case Units.None:
                return "none";
            case Units.Ppm:
                return "ppm";
            case Units.Ohm:
                return "ohms";
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
    }
}