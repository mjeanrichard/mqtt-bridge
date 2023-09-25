using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models;

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

    public async Task ProcessAsync(List<FroniusArchiveData> pvaData)
    {
        IEnumerable<string> lines = ConvertToPrometheus(pvaData);

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
}