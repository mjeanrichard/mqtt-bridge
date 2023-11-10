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

    public PrometheusClient(ILogger<PrometheusClient> logger, IOptions<PrometheusSettings> prometheusSettings)
    {
        _logger = logger;
        _prometheusSettings = prometheusSettings.Value;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_prometheusSettings.Url);
    }

    public async Task SendMetricsAsync(PushStreamContent content)
    {
        HttpRequestMessage request = new(HttpMethod.Post, "/api/v1/import/prometheus");
        request.Content = content;
        await SendAsync(request);
    }

    private async Task SendAsync(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_prometheusSettings.Username))
        {
            string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(':', _prometheusSettings.Username, _prometheusSettings.Password)));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
        }

        using HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task DeleteSeriesData(string seriesFilter)
    {
        _logger.LogInformation($"Deleting prometheus data for filter '{seriesFilter}'.");
        UriBuilder deleteUri = new(_prometheusSettings.Url);
        deleteUri.Path = "/api/v1/admin/tsdb/delete_series";
        deleteUri.Query = $"match[]={seriesFilter}";
        HttpRequestMessage request = new(HttpMethod.Post, deleteUri.Uri);
        await SendAsync(request);
    }
}