using MqttBridge.Models;
using MqttBridge.Models.Input;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Subscription;

public class FroniusDailySubscriber
{
    private readonly IPublisher _publisher;

    public FroniusDailySubscriber(IPublisher publisher)
    {
        _publisher = publisher;
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

            yield return data;

            previousPowerData = data.CumulativePerDay;
        }
    }
}