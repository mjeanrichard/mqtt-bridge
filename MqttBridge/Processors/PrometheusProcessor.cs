using System.Globalization;
using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using MqttBridge.Models.Data.GasMeter;
using MqttBridge.Models.Data.OpenMqttGateway;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Data.Remocon;
using MqttBridge.Models.Data.Sensor;

namespace MqttBridge.Processors;

public class PrometheusProcessor
{
    private readonly PrometheusClient _prometheusClient;

    private readonly ILogger<PrometheusProcessor> _logger;


    public PrometheusProcessor(PrometheusClient prometheusClient, ILogger<PrometheusProcessor> logger)
    {
        _prometheusClient = prometheusClient;
        _logger = logger;
    }

    public async Task ProcessAsync(List<EnvSensorData> envSensorData)
    {
        _logger.LogInformation($"Writing '{nameof(EnvSensorData)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(envSensorData));
    }

    public async Task ProcessAsync(List<FroniusArchiveData> pvaData)
    {
        _logger.LogInformation($"Writing '{nameof(FroniusArchiveData)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(pvaData));
    }

    public async Task ProcessAsync(List<DailyEnergyModel> data)
    {
        _logger.LogInformation($"Writing '{nameof(DailyEnergyModel)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(data));
    }

    public async Task ProcessAsync(List<EnvSensorInfo> info)
    {
        _logger.LogInformation($"Writing '{nameof(EnvSensorInfo)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(info));
    }

    public async Task ProcessAsync(List<GasMeterData> data)
    {
        _logger.LogInformation($"Writing '{nameof(GasMeterData)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(data));
    }

    public async Task ProcessAsync(List<RemoconModel> data)
    {
        _logger.LogInformation($"Writing '{nameof(RemoconModel)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(data));
    }

    public async Task ProcessAsync(List<PlantSenseData> data)
    {
        _logger.LogInformation($"Writing '{nameof(PlantSenseData)}' data to Prometheus.");
        await SendToPrometheus(ConvertToPrometheus(data));
    }

    private async Task SendToPrometheus(IEnumerable<string> lines)
    {
        PushStreamContent content = new(async stream =>
        {
            try
            {
                await using StreamWriter writer = new(stream, leaveOpen: true);
                foreach (string line in lines)
                {
                    await writer.WriteLineAsync(line);
                }

                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload data to prometheus");
            }
        });

        await _prometheusClient.SendMetricsAsync(content);
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<FroniusArchiveData> dataPoints)
    {
        foreach (FroniusArchiveData data in dataPoints)
        {
            DateTimeOffset dto = data.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();
            yield return new Metric("pva_energy_imported_joules") { Timestamp = ts, Value = data.CumulativePerDay.Imported * 3600 }.ToPrometheus();
            yield return new Metric("pva_power_importing_watts") { Timestamp = ts, Value = data.Instant.Imported }.ToPrometheus();
            yield return new Metric("pva_energy_exported_joules") { Timestamp = ts, Value = data.CumulativePerDay.Exported * 3600 }.ToPrometheus();
            yield return new Metric("pva_power_exporting_watts") { Timestamp = ts, Value = data.Instant.Exported }.ToPrometheus();
            yield return new Metric("pva_energy_produced_joules") { Timestamp = ts, Value = data.CumulativePerDay.Produced * 3600 }.ToPrometheus();
            yield return new Metric("pva_power_producing_watts") { Timestamp = ts, Value = data.Instant.Produced }.ToPrometheus();

            yield return new Metric("pva_energy_directly_consumed_joules") { Timestamp = ts, Value = data.CumulativePerDay.OhmPilotConsumed * 3600 }.SetTag("consumer", "OhmPilot").ToPrometheus();
            yield return new Metric("pva_power_directly_consuming_watts") { Timestamp = ts, Value = data.Instant.OhmPilotConsumed }.SetTag("consumer", "OhmPilot").ToPrometheus();

            yield return new Metric("pva_energy_directly_consumed_joules") { Timestamp = ts, Value = data.CumulativePerDay.DirectlyConsumed * 3600 }.SetTag("consumer", "Haus").ToPrometheus();
            yield return new Metric("pva_power_directly_consuming_watts") { Timestamp = ts, Value = data.Instant.DirectlyConsumed }.SetTag("consumer", "Haus").ToPrometheus();

            yield return new Metric("pva_temperature_celsius") { Timestamp = ts, Value = data.TemperaturePowerstage }.SetTag("device", "Powerstage").ToPrometheus();
            yield return new Metric("pva_temperature_celsius") { Timestamp = ts, Value = data.TemperatureOhmPilot1 }.SetTag("device", "OhmPilot1").ToPrometheus();
        }
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<DailyEnergyModel> dataPoints)
    {
        TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

        foreach (DailyEnergyModel data in dataPoints)
        {
            TimeSpan utcOffset = timezone.GetUtcOffset(data.TimestampUtc);

            DateTimeOffset midday = new(data.Date.Year, data.Date.Month, data.Date.Day, 12, 0, 0, utcOffset);
            long ts = midday.ToUnixTimeMilliseconds();
            yield return new Metric("pva_energy_day_imported_joules") { Timestamp = ts, Value = data.Imported * 3600 }.ToPrometheus();
            yield return new Metric("pva_energy_day_exported_joules") { Timestamp = ts, Value = data.Exported * 3600 }.ToPrometheus();
            yield return new Metric("pva_energy_day_produced_joules") { Timestamp = ts, Value = data.Produced * 3600 }.ToPrometheus();
            yield return new Metric("pva_energy_day_directly_consumed_joules") { Timestamp = ts, Value = data.OhmPilotConsumed * 3600 }.SetTag("consumer", "OhmPilot").ToPrometheus();
            yield return new Metric("pva_energy_day_directly_consumed_joules") { Timestamp = ts, Value = data.DirectlyConsumed * 3600 }.SetTag("consumer", "Haus").ToPrometheus();
        }
    }


    private IEnumerable<string> ConvertToPrometheus(IEnumerable<GasMeterData> dataPoints)
    {
        foreach (GasMeterData data in dataPoints)
        {
            DateTimeOffset dto = data.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();
            yield return new Metric("sensor_volume_cubicmeters") { Timestamp = ts, Value = data.Volume }.SetTag("device", "GasMeter").SetTag("device_id", data.DeviceId).ToPrometheus();
            yield return new Metric("sensor_battery_volts") { Timestamp = ts, Value = data.Battery }.SetTag("device", "GasMeter").SetTag("device_id", data.DeviceId).ToPrometheus();
        }
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<RemoconModel> dataPoints)
    {
        foreach (RemoconModel data in dataPoints)
        {
            DateTimeOffset dto = data.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();

            Metric CreateMetric(string name, double value) => new Metric(name) { Timestamp = ts, Value = value }.SetTag("device", "Heizung").SetTag("device_id", data.GatewayId);

            yield return CreateMetric("sensor_temperature_celsius", data.HotWaterTemperature).SetTag("name", "HotWater").ToPrometheus();
            yield return CreateMetric("sensor_temperature_celsius", data.OutsideTemperature).SetTag("name", "Outside").ToPrometheus();
            yield return CreateMetric("sensor_temperature_celsius", data.FlowTemperature).SetTag("name", "Flow").ToPrometheus();
            yield return CreateMetric("sensor_pressure_bar", data.HeatingCircuitPressure).SetTag("name", "HeatingCircuit").ToPrometheus();
            yield return CreateMetric("sensor_flame_on", data.FlameOn ? 1 : 0).SetTag("name", "Flame").ToPrometheus();
        }
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<EnvSensorInfo> dataPoints)
    {
        foreach (EnvSensorInfo info in dataPoints)
        {
            DateTimeOffset dto = info.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();
            Dictionary<string, string> tags = new() { { "device", info.Device }, { "ip", info.Ip } };
            yield return new Metric("sensor_connect_time_seconds") { Timestamp = ts, Value = info.ConnectTime, Tags = tags }.ToPrometheus();
            yield return new Metric("sensor_firmware_version") { Timestamp = ts, Value = info.FwVersion }.ToPrometheus();
            yield return new Metric("sensor_rssi_db") { Timestamp = ts, Value = info.Rssi }.ToPrometheus();
        }
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<EnvSensorData> dataPoints)
    {
        foreach (EnvSensorData data in dataPoints)
        {
            DateTimeOffset dto = data.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();

            string metricName = $"sensor_{data.Type.ToPrometheus()}_{data.Unit.ToPrometheus()}";

            Metric metric = new(metricName) { Timestamp = ts, Value = data.Value };
            metric.SetTag("device", data.Device);
            metric.SetTag("test", data.IsTestDevice.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            metric.SetTag("unit", data.Unit.ToString("G"));
            metric.SetTag("type", data.Type.ToString("G"));
            metric.SetTag("name", data.Name);
            yield return metric.ToPrometheus();
        }
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<PlantSenseData> dataPoints)
    {
        foreach (PlantSenseData data in dataPoints)
        {
            DateTimeOffset dto = data.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();

            Metric CreateMetric(string name, double value) => new Metric(name)
                {
                    Timestamp = ts,
                    Value = value
                }
                .SetTag("device", data.Name)
                .SetTag("model", data.Model)
                .SetTag("test", data.Test.ToString(CultureInfo.InvariantCulture).ToLowerInvariant())
                .SetTag("device_id", data.DeviceId);

            yield return CreateMetric("sensor_temperature_celsius", data.Temperature).ToPrometheus();
            yield return CreateMetric("sensor_humidity_percent", data.Humidity).ToPrometheus();
            yield return CreateMetric("sensor_moisture_percent", data.Moisture).ToPrometheus();
            yield return CreateMetric("sensor_battery_volts", data.Battery).ToPrometheus();
            yield return CreateMetric("sensor_rssi_db", data.Rssi).ToPrometheus();
            yield return CreateMetric("sensor_snr_ratio", data.Snr).ToPrometheus();
        }
    }
}