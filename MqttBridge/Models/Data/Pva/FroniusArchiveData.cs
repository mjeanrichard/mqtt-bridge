namespace MqttBridge.Models.Data.Pva;

public class FroniusArchiveData : IDataModel
{
    public DateTime TimestampUtc { get; set; }

    public int Seconds { get; set; }

    public PowerData CumulativePerDay { get; set; } = new();

    public PowerData Instant { get; set; } = new();

    public double TemperatureOhmPilot1 { get; set; }

    public double TemperaturePowerstage { get; set; }
}