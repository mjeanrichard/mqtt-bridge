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

        PointData.Builder lineBuilder = PointData.Builder.Measurement("EnvSensorMeasurement");

        lineBuilder.Timestamp(DateTimeOffset.FromUnixTimeSeconds(envSensorMeasurement.Timestamp), WritePrecision.S);

        lineBuilder.Tag("Name", envSensorMeasurement.Name);
        lineBuilder.Tag("Device", envSensorMeasurement.Device);

        foreach (KeyValuePair<string, JsonElement> measurement in envSensorMeasurement.Measurements)
        {
            switch (measurement.Key)
            {
                case "temp":
                    AddDouble(measurement.Value, lineBuilder, "temperature");
                    break;
                case "battery":
                    AddDouble(measurement.Value, lineBuilder, "voltage");
                    break;
                case "pressure":
                    AddDouble(measurement.Value, lineBuilder, "pressure");
                    break;
                case "humidity":
                    AddDouble(measurement.Value, lineBuilder, "humidity");
                    break;
                case "soil":
                    AddDouble(measurement.Value, lineBuilder, "moisture");
                    break;
                case "soil_raw":
                    AddDouble(measurement.Value, lineBuilder, "soil_raw");
                    break;
                case "lux":
                    AddDouble(measurement.Value, lineBuilder, "lumen");
                    break;
                case "lum_bb":
                    AddDouble(measurement.Value, lineBuilder, "lumen");
                    break;
                case "lum_ir":
                    AddDouble(measurement.Value, lineBuilder, "lumen");
                    break;
                case "tc1":
                    AddDouble(measurement.Value, lineBuilder, "temperature");
                    break;
                case "ta1":
                    AddDouble(measurement.Value, lineBuilder, "temperature");
                    break;
                default:
                    _logger.LogWarning($"Unknown measurement '{measurement.Key}'.");
                    break;
            }
        }

        return Task.FromResult<IReadOnlyCollection<PointData>>(new[] { lineBuilder.ToPointData() });
    }

    private void AddDouble(JsonElement measurement, PointData.Builder builder, string unit)
    {
        if (measurement.TryGetDouble(out double value))
        {
            builder.Field(unit, value);
        }
        else
        {
            _logger.LogWarning($"Measurement was not a Double ({unit}).");
        }
    }
}