namespace MqttBridge.Configuration;

public class MqttSettings
{
    public const string Name = "Mqtt";
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int? Port { get; set; }
    public bool UseTls { get; set; }
}