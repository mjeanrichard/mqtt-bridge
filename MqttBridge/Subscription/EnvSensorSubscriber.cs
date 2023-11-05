using System.Text.Json;
using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data;
using MqttBridge.Models.Data.Sensor;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class EnvSensorSubscriber
{
    private readonly ILogger<EnvSensorSubscriber> _logger;
    private readonly IPublisher _publisher;

    public EnvSensorSubscriber(ILogger<EnvSensorSubscriber> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task ProcessAsync(EnvSensorMeasurement message)
    {
        _logger.LogDebug("Received EnvSensor message.");
        IEnumerable<EnvSensorData> archiveDatas = Map(message);
        await _publisher.PublishAsync(archiveDatas.ToList());
    }

    public IEnumerable<EnvSensorData> Map(EnvSensorMeasurement message)
    {
        if (message.Measurements == null)
        {
            yield break;
        }

        foreach (KeyValuePair<string, JsonElement> measurement in message.Measurements)
        {
            EnvSensorData data = new();
            data.Device = message.Device;
            data.Name = message.Name;
            data.TimestampUtc = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime;
            data.IsTestDevice = message.Device.StartsWith("test", StringComparison.OrdinalIgnoreCase);

            SetValue(data, measurement.Value);
            SetUnit(data);

            yield return data;
        }
    }

    private void SetUnit(EnvSensorData data)
    {
        switch (data.Name)
        {
            case "temp":
            case "tc1":
            case "ta1":
            case "ruecklauf":
            case "vorlauf":
                data.Unit = Units.DegreesCelsius;
                data.Type = MeasurementType.Temperatur;
                break;
            case "battery":
                data.Unit = Units.Volt;
                data.Type = MeasurementType.Battery;
                break;
            case "pressure":
                data.Unit = Units.HectoPascal;
                data.Type = MeasurementType.Pressure;
                break;
            case "humidity":
                data.Unit = Units.Percent;
                data.Type = MeasurementType.Humidity;
                break;
            case "soil":
                data.Unit = Units.Percent;
                data.Type = MeasurementType.Moisture;
                break;
            case "soil_raw":
                data.Unit = Units.None;
                data.Type = MeasurementType.MoistureRaw;
                break;
            case "lux":
                data.Type = MeasurementType.Luminosity;
                data.Unit = Units.Lux;
                break;
            case "lum_bb":
                data.Unit = Units.Lux;
                data.Type = MeasurementType.LuminosityBroadband;
                break;
            case "lum_ir":
                data.Unit = Units.Lux;
                data.Type = MeasurementType.LuminosityIr;
                break;
            case "distance":
                data.Unit = Units.Meter;
                data.Type = MeasurementType.Distance;
                data.Value /= 1000.0;
                break;
            case "co2":
                data.Unit = Units.Ppm;
                data.Type = MeasurementType.Co2;
                break;
            case "gas":
                data.Unit = Units.Ohm;
                data.Type = MeasurementType.GasResistance;
                break;
            case "voc":
                data.Unit = Units.Ppm;
                data.Type = MeasurementType.Voc;
                break;
            case "raw_tc":
            case "meter_realtime":
            case "meter_current":
            case "ohmpilot_realtime":
            case "timestamp":
                break;
            default:
                _logger.LogWarning($"None Measurement ({data.Name}).");
                break;
        }
    }

    private void SetValue(EnvSensorData data, JsonElement measurement)
    {
        if (measurement.TryGetDouble(out double value))
        {
            data.Value = value;
        }
        else
        {
            _logger.LogWarning($"Measurement was not a Double ({data.Name}).");
        }
    }
}