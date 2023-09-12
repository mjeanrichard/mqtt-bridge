using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using MqttBridge.Models;
using MQTTnet.Client;
using Newtonsoft.Json;
using NSubstitute;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration.Mqtt;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;
using Silverback.Testing;

namespace MqttBridge.Tests;

public class FroniusArchiveDataConverterTests
{
    private static IInboundEnvelope<EnvSensorInfo> CreateEnvelope(EnvSensorInfo info)
    {
        IInboundEnvelope<EnvSensorInfo> envelope = Substitute.For<IInboundEnvelope<EnvSensorInfo>>();
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

        JsonMessageSerializer<FroniusArchiveData> jsonMessageSerializer = new JsonMessageSerializer<FroniusArchiveData>()
        {
           // Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        };

        MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        (object? Message, Type MessageType) archiveData = await jsonMessageSerializer.DeserializeAsync(stream, new MessageHeaderCollection(), MessageSerializationContext.Empty);

        archiveData.Should().NotBeNull();
        //archiveData!.Seconds.Should().Be(41700);
    }

    [Test]
    public async Task SampleTest()
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

        await using TestApplication app = new();
        await app.SendMqttMessage(json, "devices/philoweg/pva/archive");

        await Task.Delay(5000);
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

        _hostBuilder = Program.Configure();
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