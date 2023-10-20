using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record Zone([property: JsonPropertyName("num")] int Number,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("roomSens")]
    bool HasRoomSensor,
    [property: JsonPropertyName("geofenceDeroga")]
    bool GeofenceDerogaEnabled,
    [property: JsonPropertyName("isHidden")]
    bool IsHidden);