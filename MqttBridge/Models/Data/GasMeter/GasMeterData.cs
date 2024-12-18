﻿namespace MqttBridge.Models.Data.GasMeter;

public class GasMeterData : IDataModel
{
    public int Address { get; set; }

    public string DeviceId { get; set; } = string.Empty;

    public int Manufacturer { get; set; }

    public int Version { get; set; }

    public int Medium { get; set; }

    public int AccessNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Signature { get; set; } = string.Empty;

    public long Milliseconds { get; set; }

    public double Battery { get; set; }

    public double Volume { get; set; }

    public DateTime TimestampUtc { get; set; }
}