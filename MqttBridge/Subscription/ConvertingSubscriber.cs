using Microsoft.Extensions.Logging;
using MqttBridge.Converters;
using MqttBridge.Models.DataPoints;
using MqttBridge.Processors;
using Silverback.Messaging.Messages;

namespace MqttBridge.Subscription;

public class ConvertingSubscriber<TConverter, TMessage> where TConverter : IConverter<TMessage>
    where TMessage : class
{
    private readonly TConverter _converter;
    private readonly IEnumerable<IProcessor> _processors;
    private readonly ILogger<ConvertingSubscriber<TConverter, TMessage>> _logger;

    public ConvertingSubscriber(TConverter converter, IEnumerable<IProcessor> processors, ILogger<ConvertingSubscriber<TConverter, TMessage>> logger)
    {
        _converter = converter;
        _processors = processors;
        _logger = logger;
    }

    public async Task OnMessageReceived(IInboundEnvelope<TMessage> envelope)
    {
        _logger.LogInformation("HI");
        IReadOnlyCollection<MetricDataPoint> dataPoints = await _converter.ConvertAsync(envelope, CancellationToken.None);

        foreach (IProcessor processor in _processors)
        {
            await processor.ProcessAsync(dataPoints);
        }

        _logger.LogInformation("BYE");
    }
}