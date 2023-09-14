using System.Collections.Immutable;
using System.Text.Json;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public class EnvSensorMeasurementConverter : IConverter<EnvSensorMeasurement>
{
    private readonly ILogger<EnvSensorMeasurementConverter> _logger;

    public EnvSensorMeasurementConverter(ILogger<EnvSensorMeasurementConverter> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyCollection<PointData>> ToPointDataAsync(IInboundEnvelope<EnvSensorMeasurement> envelope, CancellationToken cancellationToken)
    {
        EnvSensorMeasurement? envSensorMeasurement = envelope.Message;
        if (envSensorMeasurement == null)
        {
            return Task.FromResult<IReadOnlyCollection<PointData>>(ImmutableArray<PointData>.Empty);
        }

        if (envSensorMeasurement.Measurements == null)
        {
            return Task.FromResult<IReadOnlyCollection<PointData>>(ImmutableArray<PointData>.Empty);
        }

        _logger.LogInformation($"Converting '{envSensorMeasurement.Name}' from '{envSensorMeasurement.Device}'.");

        List<PointData> points = new(envSensorMeasurement.Measurements.Count);
        foreach (KeyValuePair<string, JsonElement> measurement in envSensorMeasurement.Measurements)
        {
            PointData.Builder lineBuilder = PointData.Builder.Measurement("EnvSensorMeasurement");

            lineBuilder.Timestamp(DateTimeOffset.FromUnixTimeSeconds(envSensorMeasurement.Timestamp), WritePrecision.S);

            lineBuilder.Tag("device", envSensorMeasurement.Device);

            switch (measurement.Key)
            {
                case "temp":
                    AddDouble(measurement.Value, lineBuilder, "temperature", envSensorMeasurement.Name);
                    break;
                case "battery":
                    AddDouble(measurement.Value, lineBuilder, "voltage", envSensorMeasurement.Name);
                    break;
                case "pressure":
                    AddDouble(measurement.Value, lineBuilder, "pressure", envSensorMeasurement.Name);
                    break;
                case "humidity":
                    AddDouble(measurement.Value, lineBuilder, "humidity", envSensorMeasurement.Name);
                    break;
                case "soil":
                    AddDouble(measurement.Value, lineBuilder, "moisture", envSensorMeasurement.Name);
                    break;
                case "soil_raw":
                    AddDouble(measurement.Value, lineBuilder, "soil_raw", envSensorMeasurement.Name);
                    break;
                case "lux":
                    AddDouble(measurement.Value, lineBuilder, "lumen", envSensorMeasurement.Name);
                    break;
                case "lum_bb":
                    AddDouble(measurement.Value, lineBuilder, "lumen", envSensorMeasurement.Name);
                    break;
                case "lum_ir":
                    AddDouble(measurement.Value, lineBuilder, "lumen", envSensorMeasurement.Name);
                    break;
                case "tc1":
                    AddDouble(measurement.Value, lineBuilder, "temperature", envSensorMeasurement.Name);
                    break;
                case "ta1":
                    AddDouble(measurement.Value, lineBuilder, "temperature", envSensorMeasurement.Name);
                    break;
                case "co2":
                    AddDouble(measurement.Value, lineBuilder, "co2", envSensorMeasurement.Name);
                    break;
                case "distance":
                    AddDouble(measurement.Value, lineBuilder, "distance", "salz");
                    break;
                case "gas":
                    AddDouble(measurement.Value, lineBuilder, "gas", envSensorMeasurement.Name);
                    break;
                case "ruecklauf":
                    AddDouble(measurement.Value, lineBuilder, "temperature", envSensorMeasurement.Name);
                    break;
                case "voc":
                    AddDouble(measurement.Value, lineBuilder, "voc", envSensorMeasurement.Name);
                    break;
                default:
                    _logger.LogWarning($"Unknown measurement '{measurement.Key}'.");
                    break;
            }

            points.Add(lineBuilder.ToPointData());
        }

        return Task.FromResult<IReadOnlyCollection<PointData>>(points);
    }

    private void AddDouble(JsonElement measurement, PointData.Builder builder, string type, string name)
    {
        if (measurement.TryGetDouble(out double value))
        {
            builder.Tag("name", name);
            builder.Field(type, value);
        }
        else
        {
            _logger.LogWarning($"Measurement was not a Double ({type}).");
        }
    }
}