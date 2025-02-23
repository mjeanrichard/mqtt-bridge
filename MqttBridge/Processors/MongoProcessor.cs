using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MqttBridge.Models.Data;
using MqttBridge.Models.Data.GasMeter;
using MqttBridge.Models.Data.HomeAssistant;
using MqttBridge.Models.Data.OpenMqttGateway;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Data.Remocon;
using MqttBridge.Models.Data.Sensor;
using MqttBridge.Scrapers;

namespace MqttBridge.Processors;

public class MongoProcessor
{
    private readonly ILogger<MongoProcessor> _logger;

    private readonly IMongoDatabase _database;

    public MongoProcessor(MongoClientFactory clientFactory, ILogger<MongoProcessor> logger)
    {
        _logger = logger;
        _database = clientFactory.GetDatabase();
    }

    public async Task ProcessAsync(List<FroniusArchiveData> pvaData)
    {
        FilterDefinition<FroniusArchiveData> PvaFilter(FroniusArchiveData dataPoint) => Builders<FroniusArchiveData>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc);
        await UploadAsync(pvaData, "DetailedPowerData", PvaFilter);
    }

    public async Task ProcessAsync(List<DailyEnergyModel> data)
    {
        FilterDefinition<DailyEnergyModel> PvaFilter(DailyEnergyModel dataPoint) => Builders<DailyEnergyModel>.Filter.Where(x => x.Date == dataPoint.Date);
        await UploadAsync(data, "DailyEnergyData", PvaFilter);
    }

    public async Task ProcessAsync(List<EnvSensorData> envSensorData)
    {
        FilterDefinition<EnvSensorData> EnvFilter(EnvSensorData dataPoint) => Builders<EnvSensorData>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc && x.Name == dataPoint.Name && x.Device == dataPoint.Device);
        await UploadAsync(envSensorData, "Environment", EnvFilter);
    }

    public async Task ProcessAsync(List<HomeAssistantData> homeAssistantData)
    {
        FilterDefinition<HomeAssistantData> EnvFilter(HomeAssistantData dataPoint) => Builders<HomeAssistantData>.Filter.Where(x => x.LastReported == dataPoint.LastReported && x.EntityId == dataPoint.EntityId);
        await UploadAsync(homeAssistantData, "HomeAssistant", EnvFilter);
    }

    public async Task ProcessAsync(List<EnvSensorInfo> envSensorInfo)
    {
        FilterDefinition<EnvSensorInfo> EnvFilter(EnvSensorInfo dataPoint) => Builders<EnvSensorInfo>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc && x.Device == dataPoint.Device);
        await UploadAsync(envSensorInfo, "SensorInfo", EnvFilter);
    }

    public async Task ProcessAsync(List<GasMeterData> data)
    {
        await UploadAsync(data, "GasMeter", null);
    }

    public async Task ProcessAsync(List<RemoconModel> data)
    {
        await UploadAsync(data, "Heating", null);
    }

    public async Task ProcessAsync(List<PlantSenseData> data)
    {
        FilterDefinition<PlantSenseData> Filter(PlantSenseData dataPoint) => Builders<PlantSenseData>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc && x.Name == dataPoint.Name && x.Message == dataPoint.Message && x.DeviceId == dataPoint.DeviceId);
        await UploadAsync(data, "OpenMqttGateway", Filter);
    }

    public async Task ProcessAsync(List<PlantSenseWifi> data)
    {
        FilterDefinition<PlantSenseWifi> Filter(PlantSenseWifi dataPoint) => Builders<PlantSenseWifi>.Filter.Where(x => x.TimestampUtc == dataPoint.TimestampUtc && x.Name == dataPoint.Name && x.Message == dataPoint.Message && x.DeviceId == dataPoint.DeviceId);
        await UploadAsync(data, "OpenMqttGateway", Filter);
    }

    private async Task UploadAsync<T>(IEnumerable<T> data, string collectionName, Func<T, FilterDefinition<T>>? filterBuilder) where T : IDataModel
    {
        _logger.LogInformation($"Writing '{typeof(T).Name}' data to MongoDb.");
        IMongoCollection<T> collection = _database.GetCollection<T>(collectionName);

        List<WriteModel<T>> bulkOps = new(1000);
        foreach (T dataPoint in data)
        {
            if (filterBuilder != null)
            {
                ReplaceOneModel<T> upsertOne = new(filterBuilder(dataPoint), dataPoint) { IsUpsert = true };
                bulkOps.Add(upsertOne);
            }
            else
            {
                InsertOneModel<T> insertOne = new(dataPoint);
                bulkOps.Add(insertOne);
            }

            if (bulkOps.Count >= 1000)
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