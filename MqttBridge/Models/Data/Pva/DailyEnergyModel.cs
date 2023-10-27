namespace MqttBridge.Models.Data.Pva;

public class DailyEnergyModel : PowerData, IDataModel
{
    public DateTime TimestampUtc { get; set; }
    public DateOnly Date { get; set; }
}