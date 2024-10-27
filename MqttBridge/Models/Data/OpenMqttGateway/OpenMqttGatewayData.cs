namespace MqttBridge.Models.Data.OpenMqttGateway;

public abstract class OpenMqttGatewayData : IDataModel
{
    public string DeviceId { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string? Message { get; set; }

    public int Rssi { get; set; }

    public float Snr { get; set; }

    public int PfError { get; set; }

    public int PacketSize { get; set; }

    public DateTime TimestampUtc { get; set; }
}