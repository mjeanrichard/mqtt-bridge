using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MqttBridge.Clients;
using MqttBridge.Models.Data.Remocon;
using MqttBridge.Models.Input.RemoconApi;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Scrapers;

public class RemoconScraperService : IHostedService, IDisposable
{
    private static HotWaterModes Map(ValueAndOptions plantDataDhwMode)
    {
        switch (plantDataDhwMode.Value)
        {
            case 0:
                return HotWaterModes.Off;
            case 1:
                return HotWaterModes.On;
            case 2:
                return HotWaterModes.Eco;
            default:
                return HotWaterModes.Other;
        }
    }

    private readonly ILogger<RemoconScraperService> _logger;

    private readonly RemoconClient _client;

    private readonly IServiceProvider _serviceProvider;

    private readonly PeriodicTimer _timer;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private Task? _task;

    public RemoconScraperService(ILogger<RemoconScraperService> logger, RemoconClient client, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _client = client;
        _serviceProvider = serviceProvider;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(300));
    }

    private async Task UpdateData(CancellationToken cancellationToken)
    {
        using IServiceScope serviceScope = _serviceProvider.CreateScope();
        IPublisher publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();

        await _client.Login(cancellationToken);
        List<(Plant Plant, RemotePlantFeatures Feature)> plantWithFeatures = new();
        
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!plantWithFeatures.Any())
                {
                    _logger.LogInformation("Loading available plants...");
                    IReadOnlyList<Plant> plants = await _client.GetPlants(cancellationToken);

                    _logger.LogInformation("Loading features for all plants...");

                    foreach (Plant plant in plants)
                    {
                        RemotePlantFeatures? features = await _client.GetPlantFeatures(plant.GatewayId, cancellationToken);
                        if (features != null)
                        {
                            plantWithFeatures.Add((plant, features));
                        }
                    }
                }

                foreach ((Plant plant, RemotePlantFeatures feature) in plantWithFeatures)
                {
                    _logger.LogInformation($"Loading data for {plant.GatewayId}...");
                    PlantData? plantData = await _client.GetPlantData(plant.GatewayId, cancellationToken);
                    DataItemsResponse? items = await _client.GetPlantItems(plant.GatewayId, feature, cancellationToken);

                    if (plantData != null && items != null)
                    {
                        RemoconModel model = Map(plant, plantData, items);
                        await publisher.PublishAsync(model);
                    }
                    else
                    {
                        _logger.LogError("Could not load all Remocon data...");
                    }
                }

                _logger.LogInformation($"Done. Waiting for next interval.");
                await _timer.WaitForNextTickAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping service (cancellation).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update data.");
            }
        }
    }

    private RemoconModel Map(Plant plant, PlantData plantData, DataItemsResponse items)
    {
        RemoconModel model = new();

        model.PlantName = plant.Name;
        model.GatewayFirmwareVersion = plant.GatewayFirmwareVersion;
        model.GatewayId = plant.GatewayId;
        model.MqttApiVersion = plant.MqttApiVersion;
        model.TimestampUtc = DateTime.UtcNow;
        model.FlameOn = plantData.FlameSensor;
        model.HeatPumpOn = plantData.HeatPumpOn;
        model.HotWaterTemperature = (double)plantData.DhwStorageTemp;
        model.OutsideTemperature = (double)plantData.OutsideTemp;
        model.HotWaterMode = Map(plantData.DhwMode);

        foreach (DataItem item in items.Items)
        {
            switch (item.Id)
            {
                case "ChFlowTemp":
                    model.FlowTemperature = (double)item.Value;
                    break;
                case "HeatingCircuitPressure":
                    model.HeatingCircuitPressure = (double)item.Value;
                    break;
                case "IsFlameOn":
                    break;
                default:
                    _logger.LogWarning($"Unmapped Remocon Item '{item.Id}' with value {item.Value}");
                    break;
            }
        }

        return model;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ScraperService.");

        _task = Task.Run(() => UpdateData(_cancellationTokenSource.Token));
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping ScraperService.");
        _cancellationTokenSource.Cancel();
        if (_task != null)
        {
            await _task;
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}