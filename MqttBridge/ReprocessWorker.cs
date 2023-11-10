using System.Data.OleDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Processors;
using MqttBridge.Scrapers;

namespace MqttBridge;

public class ReprocessWorker
{
    private readonly ILogger<ReprocessWorker> _logger;

    private readonly MongoScraper _mongoScraper;

    private readonly CommandLineOptions _commandLineOptions;

    private readonly PrometheusClient _prometheusClient;

    public ReprocessWorker(ILogger<ReprocessWorker> logger, MongoScraper mongoScraper, CommandLineOptions commandLineOptions, PrometheusClient prometheusClient)
    {
        _logger = logger;
        _mongoScraper = mongoScraper;
        _commandLineOptions = commandLineOptions;
        _prometheusClient = prometheusClient;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        if (_commandLineOptions.Delete)
        {
            await _prometheusClient.DeleteSeriesData($"{{__name__=~\"pva_.*\"}}");
        }

        _logger.LogInformation("Reprocessing Data from mongo db");
        await _mongoScraper.ProcessPvaDetailAsync(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
        _logger.LogInformation("Reprocessed all data.");
    }

}