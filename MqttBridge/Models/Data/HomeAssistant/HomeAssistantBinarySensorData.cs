using MqttBridge.Models.Input;

namespace MqttBridge.Models.Data.HomeAssistant;

public class HomeAssistantBinarySensorData : HomeAssistantData
{
    public HomeAssistantBinarySensorData() : base()
    {
    }

    public HomeAssistantBinarySensorData(HomeAssistantMessage message, string topic, string entityName) : base(message, topic, entityName)
    {
        IsOn = string.Equals(message.State, "on", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsOn { get; set; }

    public override string MetricName => $"sensor_{DeviceClass}_binary";

    public override double Value => IsOn ? 1 : 0;
}