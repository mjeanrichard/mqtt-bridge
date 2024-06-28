namespace MqttBridge.Models.Data.OpenMqttGateway;

public abstract class OpenMqttGatewayData : IDataModel
{
    public string DeviceId { get; set; }

    public string Model { get; set; }


    public int Rssi { get; set; }

    public float Snr { get; set; }

    public int PfError { get; set; }

    public int PacketSize { get; set; }

    public DateTime TimestampUtc { get; set; }
}