using MqttBridge.Models.Input;

namespace MqttBridge.Models.Data.HomeAssistant;

public abstract class HomeAssistantData : IDataModel
{
    protected HomeAssistantData(HomeAssistantMessage message)
    {
        EntityId = message.EntityId;
        LastChanged = message.LastChanged;
        LastReported = message.LastReported;
        LastUpdated = message.LastUpdated;
        State = message.State;
        FriendlyName = message.Attributes["friendly_name"].GetString() ?? string.Empty;
        DeviceClass = message.Attributes["device_class"].GetString() ?? string.Empty;
        TimestampUtc = message.LastReported?.UtcDateTime ?? DateTime.UtcNow;
    }

    protected HomeAssistantData()
    {
        EntityId = string.Empty;
        FriendlyName = string.Empty;
        State = string.Empty;
        DeviceClass = "unknown";
    }

    public string DeviceClass { get; set; }

    public string EntityId { get; set; }

    public DateTime TimestampUtc { get; set; }

    public DateTimeOffset? LastChanged { get; set; }

    public DateTimeOffset? LastReported { get; set; }

    public DateTimeOffset? LastUpdated { get; set; }

    public string FriendlyName { get; set; }

    public string State { get; set; }

    public abstract string MetricName { get; }

    public abstract double Value { get; }

    public virtual IEnumerable<string> ToPrometheus()
    {
        DateTimeOffset dto = TimestampUtc;
        long ts = dto.ToUnixTimeMilliseconds();

        string metricName = MetricName;

        Metric metric = new(metricName) { Timestamp = ts, Value = Value };
        metric.SetTag("name", FriendlyName);
        metric.SetTag("entity_id", EntityId);
        yield return metric.ToPrometheus();
    }
}