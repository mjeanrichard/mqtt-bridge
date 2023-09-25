namespace MqttBridge.Models.Input
{
    public class FroniusDailyModel
    {
        public FroniusDailyModel()
        {
            Data = new List<PowerDataPoint>();
        }

        public DateTime Date { get; set; }

        public List<PowerDataPoint> Data { get; set; }
    }
}