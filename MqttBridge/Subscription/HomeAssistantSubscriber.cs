using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data.HomeAssistant;
using MqttBridge.Models.Input;
using Silverback.Messaging.Messages;
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

    public async Task ProcessAsync(IInboundEnvelope<HomeAssistantMessage> envelope)
    {
        if (envelope.Message == null)
        {
            _logger.LogWarning("Received empty HomeAssistantBinarySensor message.");
            return;
        }

        _logger.LogDebug("Received HomeAssistantBinarySensor message for topic '{topic}'.", envelope.ActualEndpointName);

        string[] segments = envelope.ActualEndpointName.Split("/");
        string sensorType = segments[^3];
        string sensorName = segments[^2];

        List<HomeAssistantData> data = new(1);
        switch (sensorType)
        {
            case "binary_sensor":
                data.Add(new HomeAssistantBinarySensorData(envelope.Message, envelope.ActualEndpointName, sensorName));
                break;
            default:
                _logger.LogWarning("Unknown HomeAssistant sensor type '{sensorType}'.", sensorType);
                return;
        }

        await _publisher.PublishAsync(data);
    }
}