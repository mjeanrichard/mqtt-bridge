namespace MqttBridge.Configuration;

public class MongoDbSettings
{
    public const string Name = "MongoDb";
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }
    public bool UseTls { get; set; }
}