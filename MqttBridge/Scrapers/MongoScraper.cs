using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MqttBridge.Models;
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

    private readonly MongoProcessor _mongoProcessor;

    private readonly IMongoDatabase _database;

    public MongoScraper(ILogger<MongoScraper> logger, MongoClientFactory mongoClientFactory, PrometheusProcessor prometheusProcessor, MongoProcessor mongoProcessor)
    {
        _logger = logger;
        _prometheusProcessor = prometheusProcessor;
        _mongoProcessor = mongoProcessor;
        _database = mongoClientFactory.GetDatabase();
    }

    public async Task ProcessPvaDetailAsync(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<FroniusArchiveData>(startDate, endDate, "DetailedPowerData", data => _prometheusProcessor.ProcessAsync(data));
    }

    public async Task ProcessPvaDaily(DateOnly? startDate, DateOnly? endDate)
    {
        await RebuildDailyPvaDataAsync(startDate, endDate);
        await ProcessDataModel<DailyEnergyModel>(startDate, endDate, "DailyEnergyData", data => _prometheusProcessor.ProcessAsync(data), 14);
    }

    private async Task RebuildDailyPvaDataAsync(DateOnly? startDate, DateOnly? endDate)
    {
        async Task Publisher(List<FroniusArchiveData> data)
        {
            List<DailyEnergyModel> models = new();
            DateOnly lastDate = DateOnly.MinValue;
            FroniusArchiveData? lastModel = null;
            foreach (FroniusArchiveData model in data.OrderBy(m => m.TimestampUtc))
            {
                DateTime localTime = model.TimestampUtc.ToSwissTime();
                DateOnly localDate = new(localTime.Year, localTime.Month, localTime.Day);
                if (lastModel != null && localDate > lastDate)
                {
                    _logger.LogInformation($"Creating Daily for {lastModel.TimestampUtc} / {localDate}...");
                    DailyEnergyModel dailyEnergyModel = new()
                    {
                        Date = lastDate,
                        DirectlyConsumed = lastModel.CumulativePerDay.DirectlyConsumed,
                        Exported = lastModel.CumulativePerDay.Exported,
                        Imported = lastModel.CumulativePerDay.Imported,
                        OhmPilotConsumed = lastModel.CumulativePerDay.OhmPilotConsumed,
                        Produced = lastModel.CumulativePerDay.Produced,
                        TimestampUtc = lastModel.TimestampUtc
                    };
                    models.Add(dailyEnergyModel);
                }

                lastModel = model;
                lastDate = localDate;
            }

            await _mongoProcessor.ProcessAsync(models);
        }

        await ProcessDataModel<FroniusArchiveData>(startDate, endDate, "DetailedPowerData", Publisher, 14);
    }

    public async Task ProcessSensorData(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<EnvSensorData>(startDate, endDate, "Environment", data => _prometheusProcessor.ProcessAsync(data), 14);
    }

    public async Task ProcessHeating(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<RemoconModel>(startDate, endDate, "Heating", data => _prometheusProcessor.ProcessAsync(data));
    }

    public async Task ProcessGasMeter(DateOnly? startDate, DateOnly? endDate)
    {
        await ProcessDataModel<GasMeterData>(startDate, endDate, "GasMeter", data => _prometheusProcessor.ProcessAsync(data));
    }

    private async Task ProcessDataModel<TEntity>(DateOnly? startDate, DateOnly? endDate, string collectionName, Func<List<TEntity>, Task> publisher, int daysToBatch = 1) where TEntity : IDataModel
    {
        IMongoCollection<TEntity> collection = _database.GetCollection<TEntity>(collectionName);

        (DateOnly start, DateOnly end) = await GetDateRangeAsync(startDate, endDate, collection);

        _logger.LogInformation($"Reprocessing entries from {start:dd.MM.yyy} to {end:dd.MM.yyyy}.");

        DateTime endTime = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        DateTime batchStartDate = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime batchEndDate = batchStartDate.AddDays(daysToBatch);
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
            batchEndDate = batchStartDate.AddDays(daysToBatch);
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