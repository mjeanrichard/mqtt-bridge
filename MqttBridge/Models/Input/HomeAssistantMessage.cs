using System.Text.Json;
using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input;

/**
 * {
      "entity_id": "binary_sensor.haustur",
      "state": "off",
      "attributes": {
        "device_class": "door",
        "friendly_name": "Haustür"
      },
      "last_changed": "2025-02-11T21:15:54.532895+00:00",
      "last_reported": "2025-02-11T21:15:54.532895+00:00",
      "last_updated": "2025-02-11T21:15:54.532895+00:00",
      "context": {
        "id": "01JKVDWP94JJ2SS3RV2PJAV8HK",
        "parent_id": null,
        "user_id": null
      }
    }
 */

public class HomeAssistantMessage
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("last_changed")]
    public DateTimeOffset? LastChanged { get; set; }

    [JsonPropertyName("last_reported")]
    public DateTimeOffset? LastReported { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, JsonElement> Attributes { get; set; } = new();

    [JsonPropertyName("context")]
    public HomeAssistantContext Context { get; set; } = new();

}

public class HomeAssistantContext
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }
}