namespace MqttBridge.Models.Data.Pva;

public class PowerData
{
    public double Produced { get; set; }
    public double Exported { get; set; }
    public double Imported { get; set; }
    public double OhmPilotConsumed { get; set; }
    public double DirectlyConsumed { get; set; }

}