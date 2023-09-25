namespace MqttBridge.Models;

public class FroniusArchiveData : IDataModel
{
    public DateTime TimestampUtc { get; set; }
    public int Seconds { get; set; }

    public PowerData CumulativePerDay { get; set; } = new PowerData();
    public PowerData Instant { get; set; } = new PowerData();
    public double TemperatureOhmPilot1 { get; set; }
    public double TemperaturePowerstage { get; set; }
}