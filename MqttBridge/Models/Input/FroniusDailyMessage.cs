namespace MqttBridge.Models.Input
{
    public class FroniusDailyMessage
    {
        public FroniusDailyMessage()
        {
            Data = new List<PowerDataPoint>();
        }

        public DateTime Date { get; set; }

        public List<PowerDataPoint> Data { get; set; }
    }
}