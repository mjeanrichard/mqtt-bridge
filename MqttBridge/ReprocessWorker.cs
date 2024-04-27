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
        _logger.LogInformation("Reprocessing Data from mongo db.");
        if (_commandLineOptions.Delete)
        {
            await _prometheusClient.DeleteSeriesData("{__name__=~\"pva_.*\"}");
            await _prometheusClient.DeleteSeriesData("{__name__=~\"sensor_.*\"}");
        }

        await Task.WhenAll(
            ReprocessPva(),
            ReprocessPvaDaily(),
            ReprocessGasMeter(),
            ReprocessHeating(),
            ReprocessEnvSensor());
        _logger.LogInformation("Reprocessing done.");
    }


    private async Task ReprocessPva()
    {
        await _mongoScraper.ProcessPvaDetailAsync(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }
    private async Task ReprocessPvaDaily()
    {
        await _mongoScraper.ProcessPvaDaily(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }

    private async Task ReprocessEnvSensor()
    {
        await _mongoScraper.ProcessSensorData(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }

    private async Task ReprocessGasMeter()
    {
        await _mongoScraper.ProcessGasMeter(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }

    private async Task ReprocessHeating()
    {
        await _mongoScraper.ProcessHeating(_commandLineOptions.StartDate, _commandLineOptions.EndDate);
    }
}