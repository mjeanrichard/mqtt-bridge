namespace MqttBridge.Models;

public class EnvSensorData : IDataModel
{
    public string Name { get; set; }
    public string Device { get; set; }
    public DateTime TimestampUtc { get; set; }

    public double Value { get; set; }

    public Units Unit { get; set; }
    public MeasurementType Type { get; set; }
    public bool IsTestDevice { get; set; }
}