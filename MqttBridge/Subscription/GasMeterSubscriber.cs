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
        GasMeterData data = Map(message);
        await _publisher.PublishAsync(data);
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