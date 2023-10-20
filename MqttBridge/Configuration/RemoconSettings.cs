namespace MqttBridge.Configuration;

public class RemoconSettings
{
    public const string Name = "Remocon";

    public string BaseUrl { get; set; } = "https://www.remocon-net.remotethermo.com/api/v2/";

    public string User { get; set; }

    public string Password { get; set; }
}