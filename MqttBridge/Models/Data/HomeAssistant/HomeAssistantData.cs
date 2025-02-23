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
        TimestampUtc = message.LastReported?.UtcDateTime ?? DateTime.UtcNow;
    }

    protected HomeAssistantData()
    {
    }

    public string EntityId { get; set; }

    public DateTime TimestampUtc { get; set; }

    public DateTimeOffset? LastChanged { get; set; }

    public DateTimeOffset? LastReported { get; set; }

    public DateTimeOffset? LastUpdated { get; set; }

    public string FriendlyName { get; set; }

    public string State { get; set; }
}

public class HomeAssistantBinarySensorData : HomeAssistantData
{
    public HomeAssistantBinarySensorData() : base()
    {
    }

    public HomeAssistantBinarySensorData(HomeAssistantMessage message) : base(message)
    {
        IsOn = string.Equals(message.State, "on", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsOn { get; set; }
}