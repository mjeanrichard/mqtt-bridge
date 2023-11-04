using Microsoft.Extensions.Logging;
using MqttBridge.Scrapers;

namespace MqttBridge;

public class ReprocessWorker
{
    private readonly ILogger<ReprocessWorker> _logger;

    private readonly MongoScraper _mongoScraper;

    private readonly CommandLineOptions _commandLineOptions;

    public ReprocessWorker(ILogger<ReprocessWorker> logger, MongoScraper mongoScraper, CommandLineOptions commandLineOptions)
    {
        _logger = logger;
        _mongoScraper = mongoScraper;
        _commandLineOptions = commandLineOptions;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reprocessing Data from mongo db");
        await _mongoScraper.ProcessPvaDetailAsync(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
        _logger.LogInformation("Reprocessed all data.");
    }
}