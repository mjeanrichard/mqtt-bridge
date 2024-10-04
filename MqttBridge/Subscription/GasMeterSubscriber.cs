using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data.GasMeter;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class GasMeterSubscriber
{
    private readonly ILogger<GasMeterSubscriber> _logger;

    private readonly IPublisher _publisher;

    public GasMeterSubscriber(ILogger<GasMeterSubscriber> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task ProcessAsync(GasMeterMessage message)
    {
        _logger.LogDebug("Received Gas Meter message.");
        GasMeterData gasMeterData = Map(message);
        if (gasMeterData.Volume > 0)
        {
            List<GasMeterData> data = new() { gasMeterData };
            await _publisher.PublishAsync(data);
        }
        else
        {
            _logger.LogWarning("Ignoring empty Gas Meter message.");
        }
    }

    private GasMeterData Map(GasMeterMessage message)
    {
        GasMeterData data = new();
        data.DeviceId = message.Id;
        data.AccessNumber = message.AccessNumber;
        data.Address = message.Address;
        data.Battery = message.BatteryMilivolts / 1000d;
        data.Manufacturer = message.Manufacturer;
        data.Medium = message.Medium;
        data.Milliseconds = message.Milliseconds;
        data.Signature = message.Signature;
        data.Status = message.Status;
        data.Version = message.Version;
        data.Volume = message.Volume;

        data.TimestampUtc = DateTime.UtcNow;

        return data;
    }
}