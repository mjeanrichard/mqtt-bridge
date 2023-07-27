using MqttBridge.Models.DataPoints;
using Silverback.Messaging.Messages;

namespace MqttBridge.Converters;

public interface IConverter<in TMessage> where TMessage : class
{
    ValueTask<IReadOnlyCollection<MetricDataPoint>> ConvertAsync(IInboundEnvelope<TMessage> envelope, CancellationToken cancellationToken);
}