namespace MqttBridge.Configuration;

public class PrometheusSettings
{
    public const string Name = "Prometheus";
    public string Url { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}