namespace MqttBridge.Models.Data.Pva;

public class FroniusArchiveData : IDataModel
{
    public FroniusArchiveData()
    {
        CumulativePerDay = new PowerData()
        {
            DirectlyConsumed = 0,
            Exported = 0,
            Imported = 0,
            OhmPilotConsumed = 0,
            Produced = 0
        };
    }

    public DateTime TimestampUtc { get; set; }

    public int Seconds { get; set; }

    public PowerData CumulativePerDay { get; set; }

    public PowerData Instant { get; set; } = new();

    public double? TemperatureOhmPilot1 { get; set; }

    public double? TemperaturePowerstage { get; set; }
}