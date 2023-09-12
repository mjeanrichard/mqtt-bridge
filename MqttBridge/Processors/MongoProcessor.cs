using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MqttBridge.Configuration;
using Silverback.Messaging.Messages;

namespace MqttBridge.Processors;

public class MongoProcessor<TMessage> where TMessage : class
{
    private readonly MongoDbSettings _mongoSettings;
    private readonly MongoClient _mongoClient;
    private readonly IMongoDatabase _database;

    public MongoProcessor(IOptions<MongoDbSettings> mongoSettings)
    {
        _mongoSettings = mongoSettings.Value;
        MongoClientSettings clientSettings = new();
        clientSettings.Server = new MongoServerAddress(_mongoSettings.Host);
        clientSettings.Credential = MongoCredential.CreateCredential("admin", _mongoSettings.Username, _mongoSettings.Password);
        _mongoClient = new MongoClient(clientSettings);
        _database = _mongoClient.GetDatabase(_mongoSettings.Database);
    }

    public Task ProcessAsync(IInboundEnvelope<TMessage> message)
    {
        return Task.CompletedTask;
    }
}