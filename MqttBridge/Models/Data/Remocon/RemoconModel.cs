using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttBridge.Models.Data.Remocon
{
    public class RemoconModel : IDataModel
    {
        public DateTime TimestampUtc { get; set; }
        public bool FlameOn { get; set; }
        public HotWaterModes HotWaterMode { get; set; }

        public double OutsideTemperature {get; set; }
        public double HotWaterTemperature { get; set; }
        public bool HeatPumpOn { get; set; }

        public double FlowTemperature { get; set; }

        public double HeatingCircuitPressure { get; set; }

        public string PlantName { get; set; }

        public string GatewayFirmwareVersion { get; set; }

        public string GatewayId { get; set; }

        public int MqttApiVersion { get; set; }
    }

    public enum HotWaterModes
    {
        Off,
        On,
        Eco,
        Other
    }
}
