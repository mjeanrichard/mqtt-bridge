namespace MqttBridge.Models.Data.OpenMqttGateway;

public class PlantSenseData : OpenMqttGatewayData
{
    public string Name { get; set; } = string.Empty;

    public double Temperature { get; set; }

    public double Battery { get; set; }

    public int BatteryPercent { get; set; }

    public int Moisture { get; set; }

    public int MoistureRaw { get; set; }

    public double Humidity { get; set; }

    public bool Test { get; set; }

    public int Index { get; set; }
}

public class PlantSenseWifi : OpenMqttGatewayData
{
    public string Name { get; set; } = string.Empty;

    public bool Test { get; set; }

    public int WifiRssi { get; set; }

    public int ConnectTime { get; set; }
}