using System.Text.Json;
using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input;

public class EnvSensorMeasurement
{
    public string Name { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public long Timestamp { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Measurements { get; set; }
}