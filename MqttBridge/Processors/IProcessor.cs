using MqttBridge.Models.DataPoints;

namespace MqttBridge.Processors;

public interface IProcessor
{
    Task ProcessAsync(IReadOnlyCollection<MetricDataPoint> dataPoints);
}