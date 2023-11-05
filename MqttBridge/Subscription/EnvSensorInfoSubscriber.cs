using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data;
using MqttBridge.Models.Data.Mappings;
using MqttBridge.Models.Data.Sensor;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class EnvSensorInfoSubscriber
{
    private readonly ILogger<EnvSensorInfoSubscriber> _logger;
    private readonly IPublisher _publisher;

    public EnvSensorInfoSubscriber(ILogger<EnvSensorInfoSubscriber> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task ProcessAsync(EnvSensorInfoMessage message)
    {
        _logger.LogDebug("Received EnvSensor Info message.");
        EnvSensorInfo info = Map(message);
        await _publisher.PublishAsync(info);
    }

    private EnvSensorInfo Map(EnvSensorInfoMessage message)
    {
        EnvSensorInfo info = new();
        info.Device = message.Name;
        info.TimestampUtc = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime;

        info.Mac = message.Mac;
        info.Ip = message.Ip;
        info.Rssi = message.Rssi;
        info.FwVersion = message.FwVersion;
        info.ConnectTime = message.ConnectTime;
        info.PowerGood = message.PowerGood > 0;
        info.MqttConnected = message.Mqtt > 0;
        info.SdState = message.SdState > 0;
        info.ResetReason = EnvSensorInfoMapping.MapResetReason(message.ResetReason, _logger);
        info.NtpState = EnvSensorInfoMapping.MapNtpState(message.NtpState, _logger);
        info.Wifi = EnvSensorInfoMapping.MapWifiState(message.Wifi, _logger);

        return info;
    }
}