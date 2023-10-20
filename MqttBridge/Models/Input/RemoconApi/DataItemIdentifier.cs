using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record DataItemIdentifier(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("zn")] int Zone
);