using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Processors;
using Shouldly;

namespace MqttBridge.Tests;

public class PrometheusClientTests
{
    private static PrometheusClient CreateClient(HttpMessageHandler handler)
    {
        IOptions<PrometheusSettings> settings = Options.Create(new PrometheusSettings { Url = "http://localhost/" });
        return new PrometheusClient(NullLogger<PrometheusClient>.Instance, settings, handler);
    }

    private static Func<HttpContent> ContentFactory(params string[] lines) =>
        () => new PushStreamContent(async stream =>
        {
            await using StreamWriter writer = new(stream, leaveOpen: true);
            foreach (string line in lines)
            {
                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync();
        });

    [Test]
    public async Task SendMetricsAsync_SendsOnce_WhenServerSucceeds()
    {
        StubHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await CreateClient(handler).SendMetricsAsync(ContentFactory("metric 1"));

        // Regression: the loop used to re-send even after success -> InvalidOperationException.
        handler.Requests.Count.ShouldBe(1);
        handler.Bodies[0].ShouldContain("metric 1");
    }

    [Test]
    public async Task SendMetricsAsync_RetriesWithFreshRequestAndBody_UntilSuccess()
    {
        // Fail the first two attempts, then succeed.
        StubHandler handler = new(attempt => attempt < 2
            ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
            : new HttpResponseMessage(HttpStatusCode.OK));

        await CreateClient(handler).SendMetricsAsync(ContentFactory("metric 1"));

        handler.Requests.Count.ShouldBe(3);
        // Regression: retries must use a fresh HttpRequestMessage instance each time...
        handler.Requests.Distinct().Count().ShouldBe(3);
        // ...and a freshly serialized body (single-use content stream).
        handler.Bodies.ShouldAllBe(body => body.Contains("metric 1"));
    }

    [Test]
    public async Task SendMetricsAsync_Throws_AfterExhaustingRetries()
    {
        StubHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await Should.ThrowAsync<HttpRequestException>(
            () => CreateClient(handler).SendMetricsAsync(ContentFactory("metric 1")));

        // 1 initial attempt + 3 retries.
        handler.Requests.Count.ShouldBe(4);
    }

    [Test]
    public async Task SendMetricsAsync_AddsBasicAuthHeader_WhenCredentialsConfigured()
    {
        StubHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        IOptions<PrometheusSettings> settings = Options.Create(new PrometheusSettings
        {
            Url = "http://localhost/",
            Username = "user",
            Password = "pass"
        });
        PrometheusClient client = new(NullLogger<PrometheusClient>.Instance, settings, handler);

        await client.SendMetricsAsync(ContentFactory("metric 1"));

        System.Net.Http.Headers.AuthenticationHeaderValue? auth = handler.Requests[0].Headers.Authorization;
        auth.ShouldNotBeNull();
        auth.Scheme.ShouldBe("Basic");
        auth.Parameter.ShouldBe(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:pass")));
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpResponseMessage> _responder;

        public StubHandler(Func<int, HttpResponseMessage> responder) => _responder = responder;

        public List<HttpRequestMessage> Requests { get; } = new();

        public List<string> Bodies { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int attempt = Requests.Count;
            Requests.Add(request);
            Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
            return _responder(attempt);
        }
    }
}
