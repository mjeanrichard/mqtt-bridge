namespace MqttBridge.Models;

public class EnvSensorInfo
{
    public long Timestamp { get; set; }
    public string Name { get; set; }
    public string Mac { get; set; }
    public string Ip { get; set; }
    public int Rssi { get; set; }
    public int FwVersion { get; set; }
    public int SdState { get; set; }
    public int ConnectTime { get; set; }
    public int ResetReason { get; set; }
    public int PowerGood { get; set; }
    public int NtpState { get; set; }
    public int Wifi { get; set; }
    public int Mqtt { get; set; }
}