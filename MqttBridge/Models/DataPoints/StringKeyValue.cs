namespace MqttBridge.Models.DataPoints;

public record StringKeyValue(string Name, string Value) : IMetricKeyValue
{
    public string StringValue => Value;
}