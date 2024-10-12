namespace MqttBridge.Models.Input;

public class PowerDataPoint
{
    public DateTime TimestampUtc { get; set; }
    public int Seconds { get; set; }
    public double? Produced { get; set; }
    public double? Exported { get; set; }
    public double? Imported { get; set; }
    public double? OhmPilotConsumed { get; set; }
    public double? DirectlyConsumed { get; set; }
    public double? TemperatureOhmPilot1 { get; set; }
    public double? TemperaturePowerstage { get; set; }
}