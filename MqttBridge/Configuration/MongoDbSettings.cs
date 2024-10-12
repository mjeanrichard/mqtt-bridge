namespace MqttBridge.Configuration;

public class MongoDbSettings
{
    public const string Name = "MongoDb";

    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public bool UseTls { get; set; }
}