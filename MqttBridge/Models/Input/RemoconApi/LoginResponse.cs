using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("act")] Account Account)
{
}