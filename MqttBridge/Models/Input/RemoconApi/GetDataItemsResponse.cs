using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record DataItemsResponse(
    [property: JsonPropertyName("features")]
    RemotePlantFeatures Features,
    [property: JsonPropertyName("items")] List<DataItem> Items
);