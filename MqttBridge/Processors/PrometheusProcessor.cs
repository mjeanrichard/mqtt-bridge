using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models;
using MqttBridge.Models.Data;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Data.Sensor;

namespace MqttBridge.Processors;

public class PrometheusProcessor
{
    private readonly ILogger<PrometheusProcessor> _logger;
    private readonly HttpClient _httpClient;
    private readonly PrometheusSettings _prometheusSettings;

    public PrometheusProcessor(IOptions<PrometheusSettings> prometheusSettings, ILogger<PrometheusProcessor> logger)
    {
        _prometheusSettings = prometheusSettings.Value;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task ProcessAsync(List<EnvSensorData> envSensorData)
    {
        await SendToPrometheus(ConvertToPrometheus(envSensorData));
    }


    public async Task ProcessAsync(List<FroniusArchiveData> pvaData)
    {
        await SendToPrometheus(ConvertToPrometheus(pvaData));
    }

    public async Task ProcessAsync(EnvSensorInfo info)
    {
        await SendToPrometheus(ConvertToPrometheus(info));
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

        HttpRequestMessage request = new(HttpMethod.Post, _prometheusSettings.Url);
        request.Content = content;

        string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(':', _prometheusSettings.Username, _prometheusSettings.Password)));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
        using HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();
    }

    private IEnumerable<string> ConvertToPrometheus(IEnumerable<FroniusArchiveData> dataPoints)
    {
        foreach (FroniusArchiveData data in dataPoints)
        {
            DateTimeOffset dto = data.TimestampUtc;
            long ts = dto.ToUnixTimeMilliseconds();
            yield return new Metric("pva_power_imported_joules") { Timestamp = ts, Value = data.CumulativePerDay.Imported * 3600 }.ToPrometheus();
            yield return new Metric("pva_power_importing_watts") { Timestamp = ts, Value = data.Instant.Imported }.ToPrometheus();
            yield return new Metric("pva_power_exported_joules") { Timestamp = ts, Value = data.CumulativePerDay.Exported * 3600 }.ToPrometheus();
            yield return new Metric("pva_power_exporting_watts") { Timestamp = ts, Value = data.Instant.Exported }.ToPrometheus();
            yield return new Metric("pva_power_produced_joules") { Timestamp = ts, Value = data.CumulativePerDay.Produced * 3600 }.ToPrometheus();
            yield return new Metric("pva_power_producing_watts") { Timestamp = ts, Value = data.Instant.Produced }.ToPrometheus();

            yield return new Metric("pva_power_directly_consumed_joules") { Timestamp = ts, Value = data.CumulativePerDay.OhmPilotConsumed * 3600 }.SetTag("consumer", "OhmPilot").ToPrometheus();
            yield return new Metric("pva_power_directly_consuming_watts") { Timestamp = ts, Value = data.Instant.OhmPilotConsumed }.SetTag("consumer", "OhmPilot").ToPrometheus();

            yield return new Metric("pva_power_directly_consumed_joules") { Timestamp = ts, Value = data.CumulativePerDay.DirectlyConsumed * 3600 }.SetTag("consumer", "Haus").ToPrometheus();
            yield return new Metric("pva_power_directly_consuming_watts") { Timestamp = ts, Value = data.Instant.DirectlyConsumed }.SetTag("consumer", "Haus").ToPrometheus();

            yield return new Metric("pva_temperature_celsius") { Timestamp = ts, Value = data.TemperaturePowerstage }.SetTag("device", "Powerstage").ToPrometheus();
            yield return new Metric("pva_temperature_celsius") { Timestamp = ts, Value = data.TemperatureOhmPilot1 }.SetTag("device", "OhmPilot1").ToPrometheus();
        }
    }

    private IEnumerable<string> ConvertToPrometheus(EnvSensorInfo info)
    {
        DateTimeOffset dto = info.TimestampUtc;
        long ts = dto.ToUnixTimeMilliseconds();
        Dictionary<string, string> tags = new() { { "device", info.Device }, { "ip", info.Ip } };
        yield return new Metric("sensor_connect_time_seconds") { Timestamp = ts, Value = info.ConnectTime, Tags = tags }.ToPrometheus();
        yield return new Metric("sensor_firmware_version") { Timestamp = ts, Value = info.FwVersion }.ToPrometheus();
        yield return new Metric("sensor_rssi_db") { Timestamp = ts, Value = info.Rssi }.ToPrometheus();
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
            metric.SetTag("test", data.IsTestDevice.ToString(CultureInfo.InvariantCulture));
            metric.SetTag("unit", data.Unit.ToString("G"));
            metric.SetTag("type", data.Type.ToString("G"));
            metric.SetTag("name", data.Name);
            yield return metric.ToPrometheus();
        }
    }
}

public static class PrometheusExtensions
{
    public static string ToPrometheus(this MeasurementType type)
    {
        switch (type)
        {
            case MeasurementType.Temperatur:
                return "temperature";
            case MeasurementType.Humidity:
                return "humidity";
            case MeasurementType.Moisture:
                return "moisture";
            case MeasurementType.Pressure:
                return "pressure";
            case MeasurementType.Distance:
                return "distance";
            case MeasurementType.Luminosity:
                return "luminosity";
            case MeasurementType.LuminosityBroadband:
                return "luminosity_broadband";
            case MeasurementType.LuminosityIr:
                return "luminosity_ir";
            case MeasurementType.Co2:
                return "co2";
            case MeasurementType.Voc:
                return "voc";
            case MeasurementType.GasResistance:
                return "gas_resistance";
            case MeasurementType.Battery:
                return "battery";
            case MeasurementType.MoistureRaw:
                return "moisture_raw";
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static string ToPrometheus(this Units unit)
    {
        switch (unit)
        {
            case Units.DegreesCelsius:
                return "celsius";
            case Units.Meter:
                return "meters";
            case Units.Volt:
                return "volts";
            case Units.Percent:
                return "percent";
            case Units.HectoPascal:
                return "hectopascals";
            case Units.Lux:
                return "lux";
            case Units.None:
                return "none";
            case Units.Ppm:
                return "ppm";
            case Units.Ohm:
                return "ohms";
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
    }
}