using System.Collections.Immutable;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public class EnvSensorInfoConverter : IConverter<EnvSensorInfo>
{
    private readonly ILogger<EnvSensorInfoConverter> _logger;

    public EnvSensorInfoConverter(ILogger<EnvSensorInfoConverter> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyCollection<PointData>> ToPointDataAsync(IInboundEnvelope<EnvSensorInfo> envelope, CancellationToken cancellationToken)
    {
        EnvSensorInfo? info = envelope.Message;
        if (info == null)
        {
            return Task.FromResult<IReadOnlyCollection<PointData>>(ImmutableArray<PointData>.Empty);
        }

        _logger.LogInformation($"Converting EnvSensorInfo '{info.Name}' to PointData.");

        PointData.Builder lineBuilder = PointData.Builder.Measurement("EnvSensorInfo");

        lineBuilder.Tag("Device", info.Name);
        lineBuilder.Tag("Ip", info.Ip);
        lineBuilder.Tag("Mac", info.Mac);

        lineBuilder.Field("ConnectTime", info.ConnectTime);
        lineBuilder.Field("FwVersion", info.FwVersion);
        lineBuilder.Field("Mqtt", info.Mqtt);
        lineBuilder.Field("NtpState", info.NtpState);
        lineBuilder.Field("PowerGood", info.PowerGood);
        lineBuilder.Field("ResetReason", info.ResetReason);
        lineBuilder.Field("Rssi", info.Rssi);
        lineBuilder.Field("SdState", info.SdState);
        lineBuilder.Field("Wifi", info.Wifi);

        return Task.FromResult<IReadOnlyCollection<PointData>>(new[] { lineBuilder.ToPointData() });
    }
}