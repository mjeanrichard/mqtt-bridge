using Microsoft.Extensions.Logging;
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
        _logger.LogInformation("Reprocessing Data from mongo db for PVA");
        await ReprocessPva();
        _logger.LogInformation("Reprocessing Data from mongo db for EnvSensor");
        await ReprocessEnvSensor();
        _logger.LogInformation("Reprocessing done.");
    }


    private async Task ReprocessPva()
    {
        if (_commandLineOptions.Delete)
        {
            await _prometheusClient.DeleteSeriesData("{__name__=~\"pva_.*\"}");
        }

        await _mongoScraper.ProcessPvaDetailAsync(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }

    private async Task ReprocessEnvSensor()
    {
        if (_commandLineOptions.Delete)
        {
            await _prometheusClient.DeleteSeriesData("{__name__=~ \"sensor_.*\", device!=\"Heizung\"}");
        }

        await _mongoScraper.ProcessSensorData(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }
}