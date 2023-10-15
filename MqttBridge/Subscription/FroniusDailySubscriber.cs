using Microsoft.Extensions.Logging;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class FroniusDailySubscriber
{
    private readonly IPublisher _publisher;
    private readonly ILogger<FroniusDailySubscriber> _logger;

    public FroniusDailySubscriber(IPublisher publisher, ILogger<FroniusDailySubscriber> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ProcessAsync(FroniusDailyModel message)
    {
        IEnumerable<FroniusArchiveData> archiveDatas = Map(message);
        await _publisher.PublishAsync(archiveDatas.ToList());
    }

    public IEnumerable<FroniusArchiveData> Map(FroniusDailyModel message)
    {
        PowerData previousPowerData = new();
        foreach (PowerDataPoint dataPoint in message.Data.OrderBy(d => d.Seconds))
        {
            FroniusArchiveData data = new();

            data.TimestampUtc = dataPoint.TimestampUtc;
            data.Seconds = dataPoint.Seconds;

            data.TemperatureOhmPilot1 = dataPoint.TemperatureOhmPilot1; 
            data.TemperaturePowerstage = dataPoint.TemperaturePowerstage;

            data.Instant.DirectlyConsumed = dataPoint.DirectlyConsumed * 12;
            data.Instant.Imported = dataPoint.Imported * 12;
            data.Instant.Exported = dataPoint.Exported * 12;
            data.Instant.OhmPilotConsumed = dataPoint.OhmPilotConsumed * 12;
            data.Instant.Produced = dataPoint.Produced * 12;

            data.CumulativePerDay.DirectlyConsumed = dataPoint.DirectlyConsumed + previousPowerData.DirectlyConsumed;
            data.CumulativePerDay.Imported = dataPoint.Imported + previousPowerData.Imported;
            data.CumulativePerDay.Exported = dataPoint.Exported + previousPowerData.Exported;
            data.CumulativePerDay.OhmPilotConsumed = dataPoint.OhmPilotConsumed + previousPowerData.OhmPilotConsumed;
            data.CumulativePerDay.Produced = dataPoint.Produced + previousPowerData.Produced;

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