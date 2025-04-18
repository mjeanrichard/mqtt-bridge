using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Input;
using MQTTnet.Client;
using NSubstitute;
using Shouldly;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration.Mqtt;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;
using Silverback.Testing;

namespace MqttBridge.Tests;

public class FroniusArchiveDataConverterTests
{
    private static IInboundEnvelope<EnvSensorInfoMessage> CreateEnvelope(EnvSensorInfoMessage info)
    {
        IInboundEnvelope<EnvSensorInfoMessage> envelope = Substitute.For<IInboundEnvelope<EnvSensorInfoMessage>>();
        envelope.Message.Returns(info);
        return envelope;
    }

    [Test]
    public async Task ConvertAsync_ConvertsJson()
    {
        string json = """
                      {
                          "Timestamp": "2023-09-10T11:35:00+02:00",
                          "Seconds": 41700,
                          "Produced": 584.3863888888889,
                          "Einspeisen": 38,
                          "Bezug": 0,
                          "OhmPilotConsumed": 499,
                          "DirectlyConsumed": 47.38638888888886,
                          "TotalConsumed": 47.38638888888886,
                          "TemperatureOhmPilot1": 59.5,
                          "TemperaturePowerstage": 47
                      }
                      """;

        JsonMessageSerializer<FroniusArchiveData> jsonMessageSerializer = new()
        {
            // Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        };

        MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        (object? Message, Type MessageType) archiveData = await jsonMessageSerializer.DeserializeAsync(stream, new MessageHeaderCollection(), MessageSerializationContext.Empty);

        archiveData.Message.ShouldNotBeNull();
        //archiveData!.Seconds.Should().Be(41700);
    }
}

public class TestApplication : IAsyncDisposable
{
    private readonly Task _mainTask;

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly HostApplicationBuilder _hostBuilder;

    private readonly IHost _host;

    public TestApplication()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        _hostBuilder = Program.ConfigureHost(new CommandLineOptions());
        ConfigureTestServices(_hostBuilder.Services);

        _host = _hostBuilder.Build();

        _mainTask = Task.Run(() => _host.RunAsync(_cancellationTokenSource.Token));
    }

    public IServiceProvider Services => _host.Services;

    public async Task SendMqttMessage(string message, string topic)
    {
        using IServiceScope scope = Services.CreateScope();

        MqttBroker broker = scope.ServiceProvider.GetRequiredService<MqttBroker>();
        IProducer producer = broker.GetProducer(new MqttProducerEndpoint(topic)
        {
            Configuration = new MqttClientConfig()
            {
                ClientId = "testClient",
                ChannelOptions = new MqttClientTcpOptions()
                {
                    Server = "mqtt"
                }
            }
        });

        await producer.RawProduceAsync(Encoding.UTF8.GetBytes(message));

        await WaitForMessageConsumptionAsync(scope);
    }

    private async Task WaitForMessageConsumptionAsync(IServiceScope scope)
    {
        IMqttTestingHelper testingHelper = scope.ServiceProvider.GetRequiredService<IMqttTestingHelper>();
        await testingHelper.WaitUntilAllMessagesAreConsumedAsync(TimeSpan.FromSeconds(5));
    }

    public async ValueTask Stop()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            await _mainTask;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        services.UseMockedMqtt();
    }

    public ValueTask DisposeAsync()
    {
        return Stop();
    }
}