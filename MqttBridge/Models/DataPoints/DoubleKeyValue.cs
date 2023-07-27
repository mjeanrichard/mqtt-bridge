using System.Globalization;

namespace MqttBridge.Models.DataPoints;

public record DoubleKeyValue(string Name, double Value) : IMetricKeyValue
{
    public string StringValue => Value.ToString(CultureInfo.InvariantCulture);
}