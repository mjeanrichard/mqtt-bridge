using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MqttBridge.Models.Data;
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

    public async Task ProcessPvaDetailAsync(DateOnly? startDate, DateOnly? endDate)
    {
        IMongoCollection<FroniusArchiveData> collection = _database.GetCollection<FroniusArchiveData>("DetailedPowerData");

        (DateOnly start, DateOnly end) = await GetDateRangeAsync(startDate, endDate, collection);

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

        await ProcessData(start, end, DataProcessor);
    }

    public async Task ProcessPvaDaily(DateOnly? startDate, DateOnly? endDate)
    {
        IMongoCollection<DailyEnergyModel> collection = _database.GetCollection<DailyEnergyModel>("DailyEnergyData");

        (DateOnly start, DateOnly end) = await GetDateRangeAsync(startDate, endDate, collection);

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

        await ProcessData(start, end, DataProcessor);
    }

    public async Task ProcessSensorData(DateOnly? startDate, DateOnly? endDate)
    {
        IMongoCollection<EnvSensorData> collection = _database.GetCollection<EnvSensorData>("Environment");

        (DateOnly start, DateOnly end) = await GetDateRangeAsync(startDate, endDate, collection);

        async Task DataProcessor(DateTime batchStartDate, DateTime batchEndDate)
        {
            FilterDefinition<EnvSensorData> filter = Builders<EnvSensorData>.Filter.Where(d => d.TimestampUtc >= batchStartDate && d.TimestampUtc < batchEndDate);

            IAsyncCursor<EnvSensorData> dataCursor = await collection.FindAsync(filter);
            List<EnvSensorData> data = await dataCursor.ToListAsync();

            if (data.Any())
            {
                await _prometheusProcessor.ProcessAsync(data);
            }
        }

        await ProcessData(start, end, DataProcessor);
    }
    
    private async Task<(DateOnly startDate, DateOnly endDate)> GetDateRangeAsync<TEntity>(DateOnly? startDate, DateOnly? endDate, IMongoCollection<TEntity> collection) where TEntity : IDataModel
    {
        if (!startDate.HasValue)
        {
            startDate = await GetEarliestDate(collection);
        }

        if (!endDate.HasValue)
        {
            endDate = DateOnly.FromDateTime(DateTime.Now);
        }

        return (startDate.Value, endDate.Value);
    }

    private async Task<DateOnly> GetEarliestDate<TEntity>(IMongoCollection<TEntity> collection) where TEntity : IDataModel
    {
        SortDefinition<TEntity>? sort = Builders<TEntity>.Sort.Ascending(f => f.TimestampUtc);
        FindOptions<TEntity, TEntity> findOptions = new() { Sort = sort, Projection = Builders<TEntity>.Projection.Include(e => e.TimestampUtc).Exclude(new StringFieldDefinition<TEntity>("_id")) };
        IAsyncCursor<TEntity> dataCursor = await collection.FindAsync(FilterDefinition<TEntity>.Empty, findOptions);
        TEntity? earliest = await dataCursor.FirstOrDefaultAsync();

        DateTime dateTime = earliest?.TimestampUtc ?? DateTime.Now;
        return DateOnly.FromDateTime(dateTime);
    }

    private async Task ProcessData(DateOnly startDate, DateOnly endDate, Func<DateTime, DateTime, Task> dataProcessor)
    {
        _logger.LogInformation($"Reprocessing entries from {startDate:dd.MM.yyy} to {endDate:dd.MM.yyyy}.");

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
}