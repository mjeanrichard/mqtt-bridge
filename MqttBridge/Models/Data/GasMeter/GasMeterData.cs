namespace MqttBridge.Models.Data.GasMeter;

public class GasMeterData : IDataModel
{
    public int Address { get; set; }

    public string Id { get; set; }

    public int Manufacturer { get; set; }

    public int Version { get; set; }

    public int Medium { get; set; }

    public int AccessNumber { get; set; }

    public string Status { get; set; }

    public string Signature { get; set; }

    public long Milliseconds { get; set; }

    public double Battery { get; set; }

    public double Volume { get; set; }

    public DateTime TimestampUtc { get; set; }
}