using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Converters;
using Silverback.Messaging.Messages;

namespace MqttBridge.Processors;

public class InfluxProcessor<TMessage> : IDisposable where TMessage : class
{
    private readonly IConverter<TMessage> _converter;
    private readonly ILogger<InfluxProcessor<TMessage>> _logger;

    private readonly InfluxSettings _influxSettings;
    private readonly InfluxDBClient _influxClient;
    private readonly WriteApiAsync _writeApi;

    public InfluxProcessor(IOptions<InfluxSettings> influxSettings, IConverter<TMessage> converter, ILogger<InfluxProcessor<TMessage>> logger)
    {
        _converter = converter;
        _logger = logger;
        _influxSettings = influxSettings.Value;

        InfluxDBClientOptions options = new InfluxDBClientOptions.Builder()
            .Url(_influxSettings.Url)
            .AuthenticateToken(_influxSettings.Token)
            .RemoteCertificateValidationCallback(CertificateErrorCallback)
            .Build();

        _influxClient = new InfluxDBClient(options);
        _writeApi = _influxClient.GetWriteApiAsync();
    }

    private bool CertificateErrorCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        _logger.LogWarning($"Certificate Error: '{errors:G}' of certificate '{certificate}'");

        if (chain != null)
        {
            foreach (X509ChainElement chainElement in chain.ChainElements)
            {
                foreach (X509ChainStatus status in chainElement.ChainElementStatus)
                {
                    _logger.LogWarning($"Certificate Status: {status.Status} {chainElement.Certificate}: {status.StatusInformation.Trim()}");
                }
            }
        }

        return false;
    }

    public async Task ProcessAsync(IInboundEnvelope<TMessage> message)
    {
        IReadOnlyCollection<PointData> pointData = await _converter.ToPointDataAsync(message, CancellationToken.None);
        if (pointData.Count == 0)
        {
            return;
        }

        await _writeApi.WritePointsAsync(pointData.ToList(), _influxSettings.Bucket, _influxSettings.Organization);
    }

    public void Dispose()
    {
        _influxClient.Dispose();
    }
}