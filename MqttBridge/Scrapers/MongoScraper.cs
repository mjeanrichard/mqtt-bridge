using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Data.Sensor;
using MqttBridge.Processors;

namespace MqttBridge.Scrapers;

public class MongoScraper
{
    private readonly ILogger<MongoScraper> _logger;

    private readonly PrometheusProcessor _prometheusProcessor;

    private readonly IMongoDatabase _database;

    public MongoScraper(ILogger<MongoScraper> logger, MongoClientFactory mongoClientFactory, PrometheusProcessor prometheusProcessor)
    {
        _logger = logger;
        _prometheusProcessor = prometheusProcessor;
        _database = mongoClientFactory.GetDatabase();
    }

    private async Task ProcessData(DateOnly startDate, DateOnly endDate, Func<DateTime, DateTime, Task> dataProcessor)
    {
        DateTime endTime = endDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        DateTime batchStartDate = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime batchEndDate = batchStartDate.AddDays(1);
        while (batchStartDate <= endTime)
        {
            _logger.LogInformation($"Processing Mongo Data of {batchStartDate:dd.MM.yyyy}.");

            await dataProcessor(batchStartDate, batchEndDate);

            batchStartDate = batchEndDate;
            batchEndDate = batchStartDate.AddDays(1);
        }
    }

    public async Task ProcessPvaDetailAsync(DateOnly? startDate, DateOnly? endDate)
    {
        IMongoCollection<FroniusArchiveData> collection = _database.GetCollection<FroniusArchiveData>("DetailedPowerData");

        if (!startDate.HasValue)
        {
            startDate = await GetEarliestDate(collection);
        }

        if (!endDate.HasValue)
        {
            endDate = DateOnly.FromDateTime(DateTime.Now);
        }

        _logger.LogInformation($"Reporcessing entries from {startDate:dd.MM.yyy} to {endDate:dd.MM.yyyy}.");

        async Task DataProcessor(DateTime batchStartDate, DateTime batchEndDate)
        {
            FilterDefinition<FroniusArchiveData> filter = Builders<FroniusArchiveData>.Filter.Where(d => d.TimestampUtc >= batchStartDate && d.TimestampUtc < batchEndDate);

            IAsyncCursor<FroniusArchiveData> dataCursor = await collection.FindAsync(filter);
            List<FroniusArchiveData> data = await dataCursor.ToListAsync();

            if (data.Any())
            {
                await _prometheusProcessor.ProcessAsync(data);
            }
        }

        await ProcessData(startDate.Value, endDate.Value, DataProcessor);
    }

    private async Task<DateOnly> GetEarliestDate(IMongoCollection<FroniusArchiveData> collection)
    {
        SortDefinition<FroniusArchiveData>? sort = Builders<FroniusArchiveData>.Sort.Ascending(f => f.TimestampUtc);
        IAsyncCursor<FroniusArchiveData> dataCursor = await collection.FindAsync(FilterDefinition<FroniusArchiveData>.Empty, new FindOptions<FroniusArchiveData, FroniusArchiveData>() { Sort = sort });
        FroniusArchiveData? earliest = await dataCursor.FirstOrDefaultAsync();

        DateTime dateTime = earliest?.TimestampUtc ?? DateTime.Now;
        return DateOnly.FromDateTime(dateTime);
    }

    public async Task ProcessPvaDaily(DateOnly startDate, DateOnly endDate)
    {
        IMongoCollection<DailyEnergyModel> collection = _database.GetCollection<DailyEnergyModel>("DailyEnergyData");

        async Task DataProcessor(DateTime batchStartDate, DateTime batchEndDate)
        {
            FilterDefinition<DailyEnergyModel> filter = Builders<DailyEnergyModel>.Filter.Where(d => d.TimestampUtc >= batchStartDate && d.TimestampUtc < batchEndDate);

            IAsyncCursor<DailyEnergyModel> dataCursor = await collection.FindAsync(filter);
            List<DailyEnergyModel> data = await dataCursor.ToListAsync();

            if (data.Any())
            {
                await _prometheusProcessor.ProcessAsync(data);
            }
        }

        await ProcessData(startDate, endDate, DataProcessor);
    }



    public async Task ProcessSensorData(DateOnly startDate, DateOnly endDate)
    {
        IMongoCollection<EnvSensorData> collection = _database.GetCollection<EnvSensorData>("Environment");

        DateTime endTime = endDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        DateTime batchStartDate = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime batchEndDate = batchStartDate.AddDays(1);
        while (batchStartDate <= endTime)
        {
            _logger.LogInformation($"Processing Mongo Data of {batchStartDate:d}.");
            FilterDefinition<EnvSensorData> filter = Builders<EnvSensorData>.Filter.Where(d => d.TimestampUtc >= batchStartDate && d.TimestampUtc < batchEndDate);

            IAsyncCursor<EnvSensorData> dataCursor = await collection.FindAsync(filter);
            List<EnvSensorData> data = await dataCursor.ToListAsync();

            if (data.Any())
            {
                await _prometheusProcessor.ProcessAsync(data);
            }

            batchStartDate = batchEndDate;
            batchEndDate = batchStartDate.AddDays(1);
        }
    }
}