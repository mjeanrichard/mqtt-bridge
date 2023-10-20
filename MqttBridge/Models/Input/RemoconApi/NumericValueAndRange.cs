using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record NumericValueAndRange
(
    [property: JsonPropertyName("min")] decimal Min,
    [property: JsonPropertyName("max")] decimal Max,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("step")] decimal Step
);