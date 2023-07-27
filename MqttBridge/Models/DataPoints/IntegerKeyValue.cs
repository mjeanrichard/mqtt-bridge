using System.Globalization;

namespace MqttBridge.Models.DataPoints;

public record IntegerKeyValue(string Name, int Value) : IMetricKeyValue
{
    public string StringValue => Value.ToString(CultureInfo.InvariantCulture);
}