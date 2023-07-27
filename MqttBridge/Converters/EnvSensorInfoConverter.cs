using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using MqttBridge.Models.DataPoints;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public class EnvSensorInfoConverter : IConverter<EnvSensorInfo>
{
    private readonly ILogger<EnvSensorInfoConverter> _logger;

    public EnvSensorInfoConverter(ILogger<EnvSensorInfoConverter> logger)
    {
        _logger = logger;
    }

    public ValueTask<IReadOnlyCollection<MetricDataPoint>> ConvertAsync(IInboundEnvelope<EnvSensorInfo> envelope, CancellationToken cancellationToken)
    {
        EnvSensorInfo? info = envelope.Message;
        if (info == null)
        {
            return ValueTask.FromResult<IReadOnlyCollection<MetricDataPoint>>(ImmutableArray<MetricDataPoint>.Empty);
        }

        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeSeconds(info.Timestamp);
        MetricDataPoint dataPoint = new(timestamp.DateTime, "EnvSensorInfo", info.Name);

        dataPoint.AddLabel("Ip", info.Ip);
        dataPoint.AddLabel("Mac", info.Mac);

        dataPoint.AddValue("ConnectTime", info.ConnectTime);
        dataPoint.AddValue("FwVersion", info.FwVersion);
        dataPoint.AddValue("Mqtt", info.Mqtt);
        dataPoint.AddValue("NtpState", info.NtpState);
        dataPoint.AddValue("PowerGood", info.PowerGood);
        dataPoint.AddValue("ResetReason", info.ResetReason);
        dataPoint.AddValue("Rssi", info.Rssi);
        dataPoint.AddValue("SdState", info.SdState);
        dataPoint.AddValue("Wifi", info.Wifi);

        return ValueTask.FromResult<IReadOnlyCollection<MetricDataPoint>>(new[] { dataPoint });
    }
}