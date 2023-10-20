using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record DataItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("zone")] int Zone,
    [property: JsonPropertyName("kind")] DataItemKinds Kind,
    [property: JsonPropertyName("min")] decimal Min,
    [property: JsonPropertyName("max")] decimal Max,
    [property: JsonPropertyName("step")] decimal Step,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("decimals")]
    int Decimals,
    [property: JsonPropertyName("options")]
    List<int> Options,
    [property: JsonPropertyName("optTexts")]
    List<string> OptionTexts,
    [property: JsonPropertyName("readOnly")]
    bool IsReadOnly,
    [property: JsonPropertyName("error")] bool IsError,
    [property: JsonPropertyName("invalid")]
    bool IsInvalid,
    [property: JsonPropertyName("expiresOn")]
    DateTime? ExpiresOn,
    [property: JsonPropertyName("unit")] string UnitLabel);