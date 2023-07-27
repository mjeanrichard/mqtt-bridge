using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MqttBridge.Configuration;
using MqttBridge.Models.DataPoints;

namespace MqttBridge.Processors;

public class MongoProcessor : IProcessor
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

    public async Task ProcessAsync(IReadOnlyCollection<MetricDataPoint> dataPoints)
    {
        foreach (IGrouping<string, MetricDataPoint> metric in dataPoints.GroupBy(dataPoint => dataPoint.MetricGroup))
        {
            IMongoCollection<BsonDocument> collection = _database.GetCollection<BsonDocument>(metric.Key);
            await collection.InsertManyAsync(ConvertToBson(dataPoints));
        }
    }

    private IEnumerable<BsonDocument> ConvertToBson(IReadOnlyCollection<MetricDataPoint> dataPoints)
    {
        foreach (MetricDataPoint dataPoint in dataPoints)
        {
            BsonDocument labels = new();
            foreach (IMetricKeyValue label in dataPoint.Labels)
            {
                labels.Add(new BsonElement(label.Name, GetBsonValue(label)));
            }

            BsonDocument values = new();
            foreach (IMetricKeyValue value in dataPoint.Values)
            {
                values.Add(new BsonElement(value.Name, GetBsonValue(value)));
            }

            BsonDocument document = new()
            {
                { "MetricGroup", dataPoint.MetricGroup },
                { "DeviceId", dataPoint.DeviceId },
                { "Timestamp", dataPoint.Timestamp },
                { "Labels", labels },
                { "Values", values }
            };

            yield return document;
        }
    }

    private BsonValue GetBsonValue(IMetricKeyValue metricValue)
    {
        switch (metricValue)
        {
            case IntegerKeyValue integerValue:
                return integerValue.Value;
            case DoubleKeyValue doubleValue:
                return doubleValue.Value;
            default:
                return metricValue.StringValue;
        }
    }
}