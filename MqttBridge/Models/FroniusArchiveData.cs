namespace MqttBridge.Models;

/*{
  "Timestamp": "2023-09-01T14:45:00+02:00",
  "Seconds": 53100,
  "Produced": 673.7613888888889,
  "Einspeisen": 473,
  "Bezug": 0,
  "OhmPilotConsumed": 2,
  "DirectlyConsumed": 198.76138888888886,
  "TotalConsumed": 198.76138888888886,
  "TemperatureOhmPilot1": 70.2,
  "TemperaturePowerstage": 52
}*/
public class FroniusArchiveData
{
    public DateTimeOffset Timestamp { get; set; }
    public int Seconds { get; set; }
    public double Produced { get; set; }
    public double Einspeisen { get; set; }
    public double Bezug { get; set; }
    public double OhmPilotConsumed { get; set; }
    public double DirectlyConsumed { get; set; }
    public double TotalConsumed { get; set; }
    public double TemperatureOhmPilot1 { get; set; }
    public double TemperaturePowerstage { get; set; }
}