using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record Holiday(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("from")] DateTime From,
    [property: JsonPropertyName("to")] DateTime To,
    [property: JsonPropertyName("osv")] bool IsOutOfService
);