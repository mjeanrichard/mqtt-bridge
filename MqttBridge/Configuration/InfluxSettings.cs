namespace MqttBridge.Configuration;

public class InfluxSettings
{
    public const string Name = "Influx";
    public string Url { get; set; }
    public string Token { get; set; }
    public string Organization { get; set; }
    public string Bucket { get; set; }
}