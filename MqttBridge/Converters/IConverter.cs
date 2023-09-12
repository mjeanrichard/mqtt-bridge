using InfluxDB.Client.Writes;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public interface IConverter<in TMessage> where TMessage : class
{
    Task<IReadOnlyCollection<PointData>> ToPointDataAsync(IInboundEnvelope<TMessage> envelope, CancellationToken cancellationToken);
}