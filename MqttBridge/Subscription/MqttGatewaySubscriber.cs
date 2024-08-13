using System.Text.Json;
using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data.OpenMqttGateway;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class MqttGatewaySubscriber
{
    private readonly ILogger<MqttGatewaySubscriber> _logger;

    private readonly IPublisher _publisher;

    public MqttGatewaySubscriber(ILogger<MqttGatewaySubscriber> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task ProcessAsync(MqttGatewayMessage message)
    {
        _logger.LogDebug("Received OpenMqttGateway message.");
        await PublishAsync(message);
    }

    private Task PublishAsync(MqttGatewayMessage message)
    {
        switch (message.Model)
        {
            case "PlantSense":
                return PublishPlantSense(message);
            default:
                _logger.LogWarning("Unknown OpenMqttGateway Model '{model}'.", message.Model);
                return Task.CompletedTask;
        }
    }

    private async Task PublishPlantSense(MqttGatewayMessage message)
    {
        PlantSenseData data = new();
        MapOpenMqttGatewayProperties(message, data);

        if (message.Measurements.TryGetValue("name", out JsonElement nameElement))
        {
            data.Name = nameElement.GetString() ?? string.Empty;
        }

        if (message.Measurements.TryGetValue("tempc", out JsonElement tempElement) && tempElement.TryGetDouble(out double temp))
        {
            data.Temperature = temp;
        }

        if (message.Measurements.TryGetValue("bat", out JsonElement batElement) && batElement.TryGetDouble(out double bat))
        {
            data.Battery = bat;
        }

        if (message.Measurements.TryGetValue("batPct", out JsonElement batPctElement) && batPctElement.TryGetInt32(out int batPct))
        {
            data.BatteryPercent = batPct;
        }

        if (message.Measurements.TryGetValue("hum", out JsonElement humElement) && humElement.TryGetInt32(out int humidity))
        {
            data.Humidity = humidity;
        }

        if (message.Measurements.TryGetValue("moi", out JsonElement moiElement) && moiElement.TryGetInt32(out int moisture))
        {
            data.Moisture = moisture;
        }

        if (message.Measurements.TryGetValue("moiRaw", out JsonElement moiRawElement) && moiRawElement.TryGetInt32(out int moistureRaw))
        {
            data.MoistureRaw = moistureRaw;
        }

        if (message.Measurements.TryGetValue("test", out JsonElement testElement) && testElement.ValueKind == JsonValueKind.True)
        {
            data.Test = true;
        }

        await _publisher.PublishAsync(new List<PlantSenseData>() { data });
    }

    private void MapOpenMqttGatewayProperties<T>(MqttGatewayMessage message, T data) where T : OpenMqttGatewayData, new()
    {
        data.Model = message.Model;
        data.DeviceId = message.Id;
        data.PacketSize = message.PacketSize;
        data.PfError = message.PfError;
        data.Rssi = message.Rssi;
        data.Snr = message.Snr;
        data.TimestampUtc = DateTime.UtcNow;
    }
}