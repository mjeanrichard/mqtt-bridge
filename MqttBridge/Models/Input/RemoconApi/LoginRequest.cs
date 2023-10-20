using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record LoginRequest(
    [property: JsonPropertyName("usr")] string Username,
    [property: JsonPropertyName("pwd")] string Password)
{
    [property: JsonPropertyName("notTrack")]
    public bool NoTracking { get; } = true;

    [property: JsonPropertyName("imp")]
    public bool Impersonate { get; } = false;
}