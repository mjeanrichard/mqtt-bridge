using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using MqttBridge.Models.DataPoints;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public class EnvSensorMeasurementConverter : IConverter<EnvSensorMeasurement>
{
    private readonly ILogger<EnvSensorMeasurementConverter> _logger;

    public EnvSensorMeasurementConverter(ILogger<EnvSensorMeasurementConverter> logger)
    {
        _logger = logger;
    }

    public ValueTask<IReadOnlyCollection<MetricDataPoint>> ConvertAsync(IInboundEnvelope<EnvSensorMeasurement> envelope, CancellationToken cancellationToken)
    {
        EnvSensorMeasurement? envSensorMeasurement = envelope.Message;
        if (envSensorMeasurement == null)
        {
            return ValueTask.FromResult<IReadOnlyCollection<MetricDataPoint>>(ImmutableArray<MetricDataPoint>.Empty);
        }

        if (envSensorMeasurement.Measurements == null)
        {
            return ValueTask.FromResult<IReadOnlyCollection<MetricDataPoint>>(ImmutableArray<MetricDataPoint>.Empty);
        }

        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeSeconds(envSensorMeasurement.Timestamp);
        MetricDataPoint dataPoint = new(timestamp.DateTime, "EnvSensorMeasurement", envSensorMeasurement.Device);

        dataPoint.AddLabel("name", envSensorMeasurement.Name);

        foreach (KeyValuePair<string, JsonElement> measurement in envSensorMeasurement.Measurements)
        {
            JsonElement value = measurement.Value;
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    string? stringValue = value.GetString();
                    if (stringValue != null)
                    {
                        dataPoint.AddLabel(measurement.Key, stringValue);
                    }

                    break;
                case JsonValueKind.Number:
                    if (value.TryGetInt32(out int intValue))
                    {
                        dataPoint.AddValue(measurement.Key, intValue);
                    }
                    else
                    {
                        dataPoint.AddValue(measurement.Key, value.GetDouble());
                    }

                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                default:
                    break;
            }
        }

        return ValueTask.FromResult<IReadOnlyCollection<MetricDataPoint>>(new[] { dataPoint });
    }
}