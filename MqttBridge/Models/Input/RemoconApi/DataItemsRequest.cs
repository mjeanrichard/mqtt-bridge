using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record DataItemsRequest(
    [property: JsonPropertyName("useCache")]
    bool UseCache,
    [property: JsonPropertyName("items")] List<DataItemIdentifier> Items,
    [property: JsonPropertyName("features")]
    RemotePlantFeatures Features,
    [property: JsonPropertyName("culture")]
    string Culture
);