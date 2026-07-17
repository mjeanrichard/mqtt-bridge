using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;

namespace MqttBridge.Processors;

public class PrometheusClient
{
    private readonly ILogger<PrometheusClient> _logger;

    private readonly PrometheusSettings _prometheusSettings;

    private readonly HttpClient _httpClient;

    public PrometheusClient(ILogger<PrometheusClient> logger, IOptions<PrometheusSettings> prometheusSettings, HttpMessageHandler? handler = null)
    {
        _logger = logger;
        _prometheusSettings = prometheusSettings.Value;
        _httpClient = handler is null ? new HttpClient() : new HttpClient(handler);
        _httpClient.BaseAddress = new Uri(_prometheusSettings.Url);
        _httpClient.Timeout = TimeSpan.FromSeconds(300);
    }

    private const int MaxRetries = 3;

    public Task SendMetricsAsync(Func<HttpContent> contentFactory)
    {
        return SendAsync(() => new HttpRequestMessage(HttpMethod.Post, "api/v1/import/prometheus")
        {
            Content = contentFactory()
        });
    }

    private async Task SendAsync(Func<HttpRequestMessage> requestFactory)
    {
        int retries = 0;

        while (true)
        {
            // A HttpRequestMessage (and its content) can only be sent once, so build a fresh one for every attempt.
            using HttpRequestMessage request = requestFactory();

            if (!string.IsNullOrWhiteSpace(_prometheusSettings.Username))
            {
                string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(':', _prometheusSettings.Username, _prometheusSettings.Password)));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            }

            try
            {
                using HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
                responseMessage.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception ex)
            {
                if (retries >= MaxRetries)
                {
                    throw;
                }

                retries++;
                _logger.LogWarning(ex, $"Error while pushing to Prometheus '{ex.Message}'. Will retry ({retries}/{MaxRetries}).");
            }
        }
    }

    public async Task DeleteSeriesData(string seriesFilter)
    {
        _logger.LogInformation($"Deleting prometheus data for filter '{seriesFilter}'.");
        // Use a relative URI so it resolves against the configured base address (preserving any base path),
        // consistent with SendMetricsAsync.
        string relativeUri = $"api/v1/admin/tsdb/delete_series?match[]={Uri.EscapeDataString(seriesFilter)}";
        await SendAsync(() => new HttpRequestMessage(HttpMethod.Post, new Uri(relativeUri, UriKind.Relative)));
    }
}