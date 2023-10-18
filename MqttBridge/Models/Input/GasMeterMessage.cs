using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input;

public class GasMeterMessage
{
    [JsonPropertyName("address")]
    public int Address { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("manufacturer")]
    public int Manufacturer { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("medium")]
    public int Medium { get; set; }

    [JsonPropertyName("access_no")]
    public int AccessNumber { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("millis")]
    public long Milliseconds { get; set; }

    [JsonPropertyName("battery_mv")]
    public int BatteryMilivolts { get; set; }

    [JsonPropertyName("vol_m3")]
    public double Volume { get; set; }

    [JsonPropertyName("values")]
    public GasMeterValue[] Values { get; set; }
}