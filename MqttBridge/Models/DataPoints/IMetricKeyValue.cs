namespace MqttBridge.Models.DataPoints;

public interface IMetricKeyValue
{
    public string Name { get; }

    public string StringValue { get; }
}