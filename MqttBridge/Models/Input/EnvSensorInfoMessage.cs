namespace MqttBridge.Models.Input;

public class EnvSensorInfoMessage
{
    public long Timestamp { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public int? Rssi { get; set; }
    public int FwVersion { get; set; }
    public int SdState { get; set; }
    public int ConnectTime { get; set; }
    public int ResetReason { get; set; }
    public int PowerGood { get; set; }
    public int NtpState { get; set; }
    public int Wifi { get; set; }
    public int Mqtt { get; set; }
}