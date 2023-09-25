using System.Text.Json;
using System.Text.Json.Serialization;

namespace MqttBridge.Models;

public class EnvSensorMeasurement
{
    public string Name { get; set; }
    public string Device { get; set; }
    public long Timestamp { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Measurements { get; set; }
}