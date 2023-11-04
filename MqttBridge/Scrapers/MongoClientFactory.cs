using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MqttBridge.Configuration;

namespace MqttBridge.Scrapers;

public class MongoClientFactory
{
    private readonly MongoDbSettings _mongoSettings;

    private readonly MongoClient _mongoClient;

    public MongoClientFactory(IOptions<MongoDbSettings> mongoSettings)
    {
        _mongoSettings = mongoSettings.Value;
        MongoClientSettings clientSettings = new();
        clientSettings.Server = new MongoServerAddress(_mongoSettings.Host);
        clientSettings.Credential = MongoCredential.CreateCredential("admin", _mongoSettings.Username, _mongoSettings.Password);
        clientSettings.UseTls = _mongoSettings.UseTls;
        clientSettings.AllowInsecureTls = true;
        _mongoClient = new MongoClient(clientSettings);
    }

    public MongoClient CreateClient() => _mongoClient;
    public IMongoDatabase GetDatabase() => _mongoClient.GetDatabase(_mongoSettings.Database);
}