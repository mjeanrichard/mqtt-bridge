using System.Text.Json;
using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input;

public class MqttGatewayMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("rssi")]
    public int Rssi { get; set; }

    [JsonPropertyName("snr")]
    public float Snr { get; set; }

    [JsonPropertyName("pferror")]
    public int PfError { get; set; }

    [JsonPropertyName("packetSize")]
    public int PacketSize { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Measurements { get; set; }
}