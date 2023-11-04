namespace MqttBridge.Configuration;

public class MqttSettings
{
    public const string Name = "Mqtt";

    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? Port { get; set; }
    public bool UseTls { get; set; }

    public string ClientSuffix { get; set; } = string.Empty;
}