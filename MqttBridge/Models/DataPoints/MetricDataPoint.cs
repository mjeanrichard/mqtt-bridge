namespace MqttBridge.Models.DataPoints;

public class MetricDataPoint
{
    private readonly List<IMetricKeyValue> _labels = new();
    private readonly List<IMetricKeyValue> _values = new();

    public MetricDataPoint(DateTime timestamp, string metricGroup, string deviceId)
    {
        Timestamp = timestamp;
        MetricGroup = metricGroup;
        DeviceId = deviceId;
    }

    public IEnumerable<IMetricKeyValue> Values => _values;
    public IEnumerable<IMetricKeyValue> Labels => _labels;

    public DateTime Timestamp { get; set; }
    public string MetricGroup { get; }
    public string DeviceId { get; }

    public void AddLabel(string key, string value) => _labels.Add(new StringKeyValue(key, value));
    public void AddLabel(string key, int value) => _labels.Add(new IntegerKeyValue(key, value));
    public void AddValue(string key, int value) => _values.Add(new IntegerKeyValue(key, value));
    public void AddValue(string key, double value) => _values.Add(new DoubleKeyValue(key, value));
}