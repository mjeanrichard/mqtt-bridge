using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Processors;
using Shouldly;

namespace MqttBridge.Tests;

public class PrometheusClientTests
{
    private static PrometheusClient CreateClient(HttpMessageHandler handler, string url = "http://localhost/")
    {
        IOptions<PrometheusSettings> settings = Options.Create(new PrometheusSettings { Url = url });
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

    [Test]
    public async Task DeleteSeriesData_TargetsConfiguredBasePath()
    {
        StubHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        // Configured URL carries a base path that must be preserved.
        PrometheusClient client = CreateClient(handler, "http://localhost/victoria/");

        await client.DeleteSeriesData("{__name__=~\"pva_.*\"}");

        Uri requestUri = handler.Requests[0].RequestUri!;
        requestUri.AbsolutePath.ShouldBe("/victoria/api/v1/admin/tsdb/delete_series");
        requestUri.Query.ShouldContain("match[]=");
        // The filter must be URL-encoded.
        requestUri.Query.ShouldContain(Uri.EscapeDataString("{__name__=~\"pva_.*\"}"));
    }

    [Test]
    public async Task SendMetricsAsync_TargetsConfiguredBasePath()
    {
        StubHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        PrometheusClient client = CreateClient(handler, "http://localhost/victoria/");

        await client.SendMetricsAsync(ContentFactory("metric 1"));

        handler.Requests[0].RequestUri!.AbsolutePath.ShouldBe("/victoria/api/v1/import/prometheus");
    }

    [Test]
    public async Task DeleteSeriesData_UsesDeleteUrl_WhenConfigured()
    {
        StubHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        IOptions<PrometheusSettings> settings = Options.Create(new PrometheusSettings
        {
            Url = "http://vminsert:8480/insert/0/prometheus/",
            DeleteUrl = "http://vmselect:8481/delete/0/prometheus/"
        });
        PrometheusClient client = new(NullLogger<PrometheusClient>.Instance, settings, handler);

        await client.DeleteSeriesData("{__name__=~\"pva_.*\"}");

        Uri requestUri = handler.Requests[0].RequestUri!;
        // Deletes must go to the dedicated vmselect endpoint, not the write (vminsert) host.
        requestUri.Host.ShouldBe("vmselect");
        requestUri.Port.ShouldBe(8481);
        requestUri.AbsolutePath.ShouldBe("/delete/0/prometheus/api/v1/admin/tsdb/delete_series");
        requestUri.Query.ShouldContain(Uri.EscapeDataString("{__name__=~\"pva_.*\"}"));
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
