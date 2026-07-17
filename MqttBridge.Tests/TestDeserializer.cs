using System.Text;
using System.Text.Json;
using Shouldly;
using Silverback.Messaging.Configuration.Mqtt;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;

namespace MqttBridge.Tests;

/// <summary>
/// Deserializes a JSON payload the same way the MQTT endpoints do in production
/// (see <c>SilverbackExtensions.AddMqttSubscription</c>): a fixed model type with the
/// <c>x-message-type</c> header ignored, since the devices publish plain JSON without it.
/// </summary>
public static class TestDeserializer
{
    public static async Task<T> DeserializeAsync<T>(string json, bool useCamelCase = false)
        where T : class
    {
        JsonSerializerOptions? options = useCamelCase
            ? new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            : null;

        JsonMessageDeserializer<T> deserializer = new(options, JsonMessageDeserializerTypeHeaderBehavior.Ignore);

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));

        DeserializedMessage result = await deserializer.DeserializeAsync(
            stream,
            new MessageHeaderCollection(),
            new MqttConsumerEndpoint("test", new MqttConsumerEndpointConfiguration()));

        result.Message.ShouldBeOfType<T>();
        return (T)result.Message!;
    }
}
