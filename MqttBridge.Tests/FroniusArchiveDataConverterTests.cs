using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Input;
using MqttBridge.Subscription;
using NSubstitute;
using Shouldly;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;

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

        FroniusArchiveData archiveData = await TestDeserializer.DeserializeAsync<FroniusArchiveData>(json);

        archiveData.ShouldNotBeNull();
        //archiveData.Seconds.Should().Be(41700);
    }
    
    [Test]
    public async Task ConvertAsync_ConvertsJson_WhenDataIsBroken()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MqttBridge.Tests.FroniusData.json");

        FroniusDailyMessage froniusDailyMessage = await JsonSerializer.DeserializeAsync<FroniusDailyMessage>(stream);
        FroniusDetailSubscriber froniusDetailSubscriber = new(Substitute.For<IPublisher>(), NullLogger<FroniusDetailSubscriber>.Instance);
        IEnumerable<FroniusArchiveData> data = froniusDetailSubscriber.Map(froniusDailyMessage);

        data.ShouldNotBeNull();
    }
}