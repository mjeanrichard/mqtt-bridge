using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MqttBridge.Configuration;
using MqttBridge.Models.Data;
using MqttBridge.Models.Data.GasMeter;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Data.Sensor;

namespace MqttBridge.Processors;

public class MongoProcessor
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
        clientSettings.UseTls = _mongoSettings.UseTls;
        clientSettings.AllowInsecureTls = true;
        _mongoClient = new MongoClient(clientSettings);
        _database = _mongoClient.GetDatabase(_mongoSettings.Database);
    }

    public async Task ProcessAsync(List<FroniusArchiveData> pvaData)
    {
        FilterDefinition<FroniusArchiveData> PvaFilter(FroniusArchiveData dataPoint) => Builders<FroniusArchiveData>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc);
        await UploadAsync(pvaData, "DetailedPowerData", PvaFilter);
    }

    public async Task ProcessAsync(List<EnvSensorData> envSensorData)
    {
        FilterDefinition<EnvSensorData> EnvFilter(EnvSensorData dataPoint) => Builders<EnvSensorData>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc && x.Name == dataPoint.Name && x.Device == dataPoint.Device);
        await UploadAsync(envSensorData, "Environment", EnvFilter);
    }

    public async Task ProcessAsync(EnvSensorInfo envSensorInfo)
    {
        FilterDefinition<EnvSensorInfo> EnvFilter(EnvSensorInfo dataPoint) => Builders<EnvSensorInfo>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc && x.Device == dataPoint.Device);
        await UploadAsync(new[] { envSensorInfo }, "SensorInfo", EnvFilter);
    }

    public async Task ProcessAsync(GasMeterData data)
    {
        FilterDefinition<GasMeterData> EnvFilter(GasMeterData dataPoint) => Builders<GasMeterData>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc);
        await UploadAsync(new[] { data }, "GasMeter", EnvFilter);
    }

    private async Task UploadAsync<T>(IEnumerable<T> data, string collectionName, Func<T, FilterDefinition<T>> filterBuilder) where T : IDataModel
    {
        IMongoCollection<T> collection = _database.GetCollection<T>(collectionName);

        List<WriteModel<T>> bulkOps = new(100);
        foreach (T dataPoint in data)
        {
            ReplaceOneModel<T> upsertOne = new(filterBuilder(dataPoint), dataPoint) { IsUpsert = true };
            bulkOps.Add(upsertOne);

            if (bulkOps.Count >= 100)
            {
                await collection.BulkWriteAsync(bulkOps);
                bulkOps.Clear();
            }
        }

        if (bulkOps.Count > 0)
        {
            await collection.BulkWriteAsync(bulkOps);
        }
    }
}