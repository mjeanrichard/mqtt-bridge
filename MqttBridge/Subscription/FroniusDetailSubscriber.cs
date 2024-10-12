using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class FroniusDetailSubscriber
{
    private static double? ToWattHours(double? watts)
    {
        if (watts == null)
        {
            return null;
        }

        return watts.Value * 12;
    }

    private readonly IPublisher _publisher;

    private readonly ILogger<FroniusDetailSubscriber> _logger;

    public FroniusDetailSubscriber(IPublisher publisher, ILogger<FroniusDetailSubscriber> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ProcessAsync(FroniusDailyMessage message)
    {
        IEnumerable<FroniusArchiveData> archiveDatas = Map(message).ToList();
        await _publisher.PublishAsync(archiveDatas);
    }

    public IEnumerable<DailyEnergyModel> Map(IEnumerable<FroniusArchiveData> message)
    {
        foreach (FroniusArchiveData data in message)
        {
            DailyEnergyModel dailyEnergyModel = new()
            {
                Date = DateOnly.FromDateTime(data.TimestampUtc.ToSwissTime()),
                DirectlyConsumed = data.CumulativePerDay.DirectlyConsumed,
                Exported = data.CumulativePerDay.Exported,
                Imported = data.CumulativePerDay.Imported,
                OhmPilotConsumed = data.CumulativePerDay.OhmPilotConsumed,
                Produced = data.CumulativePerDay.Produced,
                TimestampUtc = data.TimestampUtc
            };
            yield return dailyEnergyModel;
        }
    }

    public IEnumerable<FroniusArchiveData> Map(FroniusDailyMessage message)
    {
        _logger.LogDebug("Received Fronius Detail message.");
        PowerData previousPowerData = new();
        foreach (PowerDataPoint dataPoint in message.Data.OrderBy(d => d.Seconds))
        {
            FroniusArchiveData data = new();

            data.TimestampUtc = dataPoint.TimestampUtc;
            data.Seconds = dataPoint.Seconds;

            data.TemperatureOhmPilot1 = dataPoint.TemperatureOhmPilot1;
            data.TemperaturePowerstage = dataPoint.TemperaturePowerstage;

            data.Instant.DirectlyConsumed = ToWattHours(dataPoint.DirectlyConsumed);
            data.Instant.Imported = ToWattHours(dataPoint.Imported);
            data.Instant.Exported = ToWattHours(dataPoint.Exported);
            data.Instant.OhmPilotConsumed = ToWattHours(dataPoint.OhmPilotConsumed);
            data.Instant.Produced = ToWattHours(dataPoint.Produced);

            data.CumulativePerDay.DirectlyConsumed = dataPoint.DirectlyConsumed.GetValueOrDefault(0) + previousPowerData.DirectlyConsumed.GetValueOrDefault(0);
            data.CumulativePerDay.Imported = dataPoint.Imported.GetValueOrDefault(0) + previousPowerData.Imported.GetValueOrDefault(0);
            data.CumulativePerDay.Exported = dataPoint.Exported.GetValueOrDefault(0) + previousPowerData.Exported.GetValueOrDefault(0);
            data.CumulativePerDay.OhmPilotConsumed = dataPoint.OhmPilotConsumed.GetValueOrDefault(0) + previousPowerData.OhmPilotConsumed.GetValueOrDefault(0);
            data.CumulativePerDay.Produced = dataPoint.Produced.GetValueOrDefault(0) + previousPowerData.Produced.GetValueOrDefault(0);

            if (data.Instant.DirectlyConsumed > 10_000)
            {
                _logger.LogWarning($"Broken Value detected: Instant '{data.Instant.DirectlyConsumed}' @ {dataPoint.Seconds}.");
                yield break;
            }

            if (data.CumulativePerDay.DirectlyConsumed > 100_000_000)
            {
                _logger.LogWarning($"Broken Value detected: CumulativePerDay '{data.CumulativePerDay.DirectlyConsumed}' Data point was '{dataPoint.DirectlyConsumed}' @ {dataPoint.Seconds}.");
                yield break;
            }

            yield return data;

            previousPowerData = data.CumulativePerDay;
        }
    }
}