using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input;

public class MqttGatewayGenericMessage
{
    [JsonPropertyName("hex")]
    public string? Hex { get; set; }

    [JsonPropertyName("rssi")]
    public int Rssi { get; set; }

    [JsonPropertyName("snr")]
    public float Snr { get; set; }

    [JsonPropertyName("pferror")]
    public int PfError { get; set; }

    [JsonPropertyName("packetSize")]
    public int PacketSize { get; set; }

}