namespace MqttBridge;

public class CommandLineOptions
{
    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool Mqtt { get; set; }
    public bool Remocon { get; set; }

    public bool Republish { get; set; }

    public bool Delete { get; set; }
}