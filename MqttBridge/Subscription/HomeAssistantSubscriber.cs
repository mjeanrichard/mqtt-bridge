using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data.HomeAssistant;
using MqttBridge.Models.Data.Sensor;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class HomeAssistantSubscriber
{
    private readonly ILogger<HomeAssistantSubscriber> _logger;

    private readonly IPublisher _publisher;

    public HomeAssistantSubscriber(ILogger<HomeAssistantSubscriber> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task ProcessAsync(HomeAssistantMessage message)
    {
        _logger.LogDebug("Received HomeAssistantBinarySensor message.");
        List<HomeAssistantBinarySensorData> data = new() { new HomeAssistantBinarySensorData(message) };
        await _publisher.PublishAsync(data);
    }
}