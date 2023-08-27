using System.Text.Json;
using MQTTnet.Protocol;
using Silverback.Messaging.Configuration.Mqtt;
using Silverback.Messaging.Serialization;

namespace MqttBridge.Subscription;

public static class SilverbackExtensions
{
    private static JsonMessageSerializer<TMessage> CreateSerializer<TMessage>()
    {
        return new JsonMessageSerializer<TMessage>()
        {
            Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        };
    }

    public static IMqttEndpointsConfigurationBuilder AddMqttInbound<TMessage>(this IMqttEndpointsConfigurationBuilder builder, string topic, string clientId)
    {
        builder.AddInbound(
            endpoint => endpoint
                .Configure(builder => builder.WithClientId(clientId))
                .ConsumeFrom(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .DisableMessageValidation()
                .UseSerializer(CreateSerializer<TMessage>())
                .OnError(policy => policy.Skip()));

        return builder;
    }
}