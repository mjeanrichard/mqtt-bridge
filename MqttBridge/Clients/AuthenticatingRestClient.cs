using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MqttBridge.Clients;

public abstract class AuthenticatingRestClient
{
    private readonly HttpClient _httpClient;

    protected AuthenticatingRestClient(string baseUrl)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    protected async Task<TResult?> UnauthenticatedPost<TRequest, TResult>(TRequest request, string url, CancellationToken cancellationToken)
    {
        HttpRequestMessage RequestBuilder()
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, url);
            requestMessage.Content = JsonContent.Create(request);
            return requestMessage;
        }

        return await Send<TResult>(RequestBuilder, false, cancellationToken);
    }

    protected async Task<TResult?> Post<TRequest, TResult>(TRequest request, string url, CancellationToken cancellationToken)
    {
        HttpRequestMessage RequestBuilder()
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, url);
            requestMessage.Content = JsonContent.Create(request);
            return requestMessage;
        }

        return await Send<TResult>(RequestBuilder, true, cancellationToken);
    }

    protected async Task<TResult?> Get<TResult>(string url, CancellationToken cancellationToken)
    {
        HttpRequestMessage RequestBuilder()
        {
            return new HttpRequestMessage(HttpMethod.Get, url);
        }

        return await Send<TResult>(RequestBuilder, true, cancellationToken);
    }

    protected abstract Task RefreshAuthentication(CancellationToken cancellationToken);

    protected abstract Task AuthenticateRequest(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    private async Task<TResult?> Send<TResult>(Func<HttpRequestMessage> requestBuilder, bool authenticationRequired, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await Send(requestBuilder, authenticationRequired, cancellationToken);
        try
        {
            if (authenticationRequired && response.StatusCode == HttpStatusCode.MethodNotAllowed)
            {
                response.Dispose();
                await RefreshAuthentication(cancellationToken);
                response = await Send(requestBuilder, authenticationRequired, cancellationToken);
            }

            response.EnsureSuccessStatusCode();

            await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<TResult>(responseStream, JsonSerializerOptions.Default, cancellationToken);
        }
        finally
        {
            response.Dispose();
        }
    }

    private async Task<HttpResponseMessage> Send(Func<HttpRequestMessage> requestBuilder, bool authenticationRequired, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = requestBuilder();
        if (authenticationRequired)
        {
            await AuthenticateRequest(request, cancellationToken);
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }
}