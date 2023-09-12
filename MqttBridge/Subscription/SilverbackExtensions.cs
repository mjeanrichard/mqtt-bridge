using System.Text.Json;
using MQTTnet.Protocol;
using Silverback.Messaging.Configuration.Mqtt;
using Silverback.Messaging.Serialization;

namespace MqttBridge.Subscription;

public static class SilverbackExtensions
{
    public static IMqttEndpointsConfigurationBuilder AddMqttInbound<TMessage>(this IMqttEndpointsConfigurationBuilder builder, string topic, string clientId, IMessageSerializer serializer)
    {
        builder.AddInbound(
            endpoint => endpoint
                .Configure(endpointBuilder => endpointBuilder.WithClientId(clientId))
                .ConsumeFrom(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .DisableMessageValidation()
                .UseSerializer(serializer)
                .OnError(policy => policy.Skip()));

        return builder;
    }
}