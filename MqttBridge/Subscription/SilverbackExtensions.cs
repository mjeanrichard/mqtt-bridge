using System.Text.Json;
using Silverback.Messaging.Configuration.Mqtt;

namespace MqttBridge.Subscription;

public static class SilverbackExtensions
{
    public static MqttClientsConfigurationBuilder AddMqttSubscription<TMessage>(this MqttClientsConfigurationBuilder builder, string clientId, string topic, bool useCamelCase)
        where TMessage : class
    {
        builder.AddClient(client => client.WithClientId(clientId)
            .Consume(endpoint =>
            {
                endpoint.ConsumeFrom(topic)
                    .WithAtLeastOnceQoS()
                    .DisableMessageValidation()
                    .OnError(policy => policy.Skip());

                // The devices publish plain JSON without Silverback's x-message-type header, so pin the
                // target type explicitly and ignore the (absent) header instead of relying on it.
                endpoint.DeserializeJson(deserializer =>
                {
                    deserializer.UseModel<TMessage>().IgnoreMessageTypeHeader();

                    if (useCamelCase)
                    {
                        deserializer.Configure(options => { options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
                    }
                });
            })
        );
        return builder;
    }
}