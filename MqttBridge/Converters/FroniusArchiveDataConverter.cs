using System.Collections.Immutable;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using MqttBridge.Models;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public class FroniusArchiveDataConverter : IConverter<FroniusArchiveData>
{
    private readonly ILogger<FroniusArchiveDataConverter> _logger;

    public FroniusArchiveDataConverter(ILogger<FroniusArchiveDataConverter> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyCollection<PointData>> ToPointDataAsync(IInboundEnvelope<FroniusArchiveData> envelope, CancellationToken cancellationToken)
    {
        FroniusArchiveData? data = envelope.Message;
        if (data == null)
        {
            return Task.FromResult<IReadOnlyCollection<PointData>>(ImmutableArray<PointData>.Empty);
        }

        _logger.LogInformation($"Converting FroniusArchiveData from {data.Timestamp}.");

        PointData.Builder lineBuilder = PointData.Builder.Measurement("archive");

        lineBuilder.Timestamp(data.Timestamp, WritePrecision.S);

        lineBuilder.Field("Bezug", data.Bezug);
        lineBuilder.Field("DirectlyConsumed", data.DirectlyConsumed);
        lineBuilder.Field("Einspeisen", data.Einspeisen);
        lineBuilder.Field("OhmPilotConsumed", data.OhmPilotConsumed);
        lineBuilder.Field("Produced", data.Produced);
        lineBuilder.Field("TemperatureOhmPilot1", data.TemperatureOhmPilot1);
        lineBuilder.Field("TemperaturePowerstage", data.TemperaturePowerstage);
        lineBuilder.Field("TotalConsumed", data.TotalConsumed);

        return Task.FromResult<IReadOnlyCollection<PointData>>(new[] { lineBuilder.ToPointData() });
    }

}