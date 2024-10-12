namespace MqttBridge.Models.Data.Sensor
{
    public class EnvSensorData : IDataModel
    {
        public string Name { get; set; } = string.Empty;

        public string Device { get; set; } = string.Empty;

        public DateTime TimestampUtc { get; set; }

        public double Value { get; set; }

        public Units Unit { get; set; }

        public MeasurementType Type { get; set; }

        public bool IsTestDevice { get; set; }
    }
}