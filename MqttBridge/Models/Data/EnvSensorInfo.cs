﻿namespace MqttBridge.Models.Data;

public class EnvSensorInfo : IDataModel
{
    public string Device { get; set; }
    public string Mac { get; set; }
    public string Ip { get; set; }
    public int Rssi { get; set; }
    public int FwVersion { get; set; }
    public bool SdState { get; set; }
    public int ConnectTime { get; set; }
    public ResetReason ResetReason { get; set; }
    public bool PowerGood { get; set; }
    public NtpState NtpState { get; set; }
    public WifiState Wifi { get; set; }
    public bool MqttConnected { get; set; }
    public DateTime TimestampUtc { get; set; }
}