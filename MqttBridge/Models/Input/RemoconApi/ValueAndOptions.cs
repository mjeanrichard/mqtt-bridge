using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record ValueAndOptions
(
    [property: JsonPropertyName("allowedOptions")]
    IList<int> AllowedOptions,
    [property: JsonPropertyName("value")] int Value
);