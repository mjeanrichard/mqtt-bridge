using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MqttBridge.Models.Data;
using MqttBridge.Models.Data.GasMeter;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Data.Remocon;
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
        await ProcessDataModel<FroniusArchiveData>(startDate, endDate, "DetailedPowerData", data => _prometheusProcessor.ProcessAsync(data));
    }

    public async Task ProcessPvaDaily(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<DailyEnergyModel>(startDate, endDate, "DailyEnergyData", data => _prometheusProcessor.ProcessAsync(data));
    }

    public async Task ProcessSensorData(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<EnvSensorData>(startDate, endDate, "Environment", data => _prometheusProcessor.ProcessAsync(data));
    }

    public async Task ProcessHeating(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<RemoconModel>(startDate, endDate, "Heating", data => _prometheusProcessor.ProcessAsync(data));
    }

    public async Task ProcessGasMeter(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<GasMeterData>(startDate, endDate, "GasMeter", data => _prometheusProcessor.ProcessAsync(data));
    }

    private async Task ProcessDataModel<TEntity>(DateOnly? startDate, DateOnly? endDate, string collectionName, Func<List<TEntity>, Task> publisher) where TEntity : IDataModel
    {
        IMongoCollection<TEntity> collection = _database.GetCollection<TEntity>(collectionName);

        (DateOnly start, DateOnly end) = await GetDateRangeAsync(startDate, endDate, collection);

        _logger.LogInformation($"Reprocessing entries from {start:dd.MM.yyy} to {end:dd.MM.yyyy}.");

        DateTime endTime = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        DateTime batchStartDate = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime batchEndDate = batchStartDate.AddDays(1);
        while (batchStartDate <= endTime)
        {
            _logger.LogInformation($"Processing '{collectionName}' data of {batchStartDate:dd.MM.yyyy}.");

            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Where(d => d.TimestampUtc >= batchStartDate && d.TimestampUtc < batchEndDate);

            IAsyncCursor<TEntity> dataCursor = await collection.FindAsync(filter);
            List<TEntity> data = await dataCursor.ToListAsync();

            if (data.Any())
            {
                await publisher(data);
            }

            batchStartDate = batchEndDate;
            batchEndDate = batchStartDate.AddDays(1);
        }
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
}