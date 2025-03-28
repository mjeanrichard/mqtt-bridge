using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using MqttBridge.Models.Data.OpenMqttGateway;
using MqttBridge.Models.Input;
using MqttBridge.Subscription;
using NSubstitute;
using Shouldly;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Messaging.Serialization;

namespace MqttBridge.Tests;

public class MqttGatewayTests
{
    private static async Task<T> DeserializeMessage<T>(string json)
    {
        JsonMessageSerializer<T> jsonMessageSerializer = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        (object? message, Type messageType) = await jsonMessageSerializer.DeserializeAsync(stream, new MessageHeaderCollection(), MessageSerializationContext.Empty);

        message.ShouldBeOfType<T>();
        T data = (T)message!;
        return data;
    }

    [Test]
    public async Task HexMessageDeserialize_ShouldWork()
    {
        string json = """
                      {
                          "hex":"7B226D6F64656C223A22506C616E7453656E7365222C226D7367223A2264617461222C226964223A22663066356264633139353863222C226E616D65223A2242C3BC726F222C2274656D7063223A32322E322C2268756D223A38322C22626174223A332E38342C22626174506374223A35312C226D6F69223A34362C226D6F69526177223A313739302C2274657374223A66616C73652C22696478223A3233302C2276223A32317D",
                          "rssi":-109,
                          "snr":7.75,
                          "pferror":2122,
                          "packetSize":168
                      }
                      """;

        MqttGatewayGenericMessage data = await DeserializeMessage<MqttGatewayGenericMessage>(json);
        data.Hex.ShouldNotBeNullOrEmpty();
        data.Rssi.ShouldBe(-109);
        data.Snr.ShouldBe(7.75f);
        data.PfError.ShouldBe(2122);
        data.PacketSize.ShouldBe(168);
    }

    [Test]
    public async Task HexMessageSubscribe_ShouldPublishCorrectly()
    {
        string json = """
                      {
                          "hex":"7B226D6F64656C223A22506C616E7453656E7365222C226D7367223A2264617461222C226964223A22663066356264633139353863222C226E616D65223A2242C3BC726F222C2274656D7063223A32322E322C2268756D223A38322C22626174223A332E38342C22626174506374223A35312C226D6F69223A34362C226D6F69526177223A313739302C2274657374223A66616C73652C22696478223A3233302C2276223A32317D",
                          "rssi":-109,
                          "snr":7.75,
                          "pferror":2122,
                          "packetSize":168
                      }
                      """;

        IPublisher publisher = Substitute.For<IPublisher>();

        List<PlantSenseData> results = new();
        //_publisher.PublishAsync(new List<PlantSenseData>() { data });
        publisher.PublishAsync(Arg.Do<List<PlantSenseData>>(lists => results.AddRange(lists))).Returns(Task.CompletedTask);

        MqttGatewaySubscriber subscriber = new(NullLogger<MqttGatewaySubscriber>.Instance, publisher);

        await subscriber.ProcessAsync(await DeserializeMessage<MqttGatewayGenericMessage>(json));

        await publisher.Received(1).PublishAsync(Arg.Any<List<PlantSenseData>>());

        PlantSenseData data = results.ShouldHaveSingleItem();
        data.Model.ShouldBe("PlantSense");
        data.Battery.ShouldBe(3.84);
        data.DeviceId.ShouldBe("f0f5bdc1958c");
        data.Name.ShouldBe("Büro");
        data.Snr.ShouldBe(7.75f);
    }


    [Test]
    public async Task MessageSubscribe_ShouldPublishCorrectly()
    {
        string json = """
                      {
                          "model":"PlantSense",
                          "msg":"data",
                          "id":"3c84279d2150",
                          "name":"aprikose",
                          "tempc":9,
                          "hum":95,
                          "bat":4,
                          "batPct":75,
                          "moi":82,
                          "moiRaw":1064,
                          "test":false,
                          "idx":62,
                          "rssi":-81,
                          "snr":10.25,
                          "pferror":2206,
                          "packetSize":162
                      }
                      """;

        IPublisher publisher = Substitute.For<IPublisher>();

        List<PlantSenseData> results = new();
        //_publisher.PublishAsync(new List<PlantSenseData>() { data });
        publisher.PublishAsync(Arg.Do<List<PlantSenseData>>(lists => results.AddRange(lists))).Returns(Task.CompletedTask);

        MqttGatewaySubscriber subscriber = new(NullLogger<MqttGatewaySubscriber>.Instance, publisher);

        await subscriber.ProcessAsync(await DeserializeMessage<MqttGatewayDeviceIdMessage>(json));

        await publisher.Received(1).PublishAsync(Arg.Any<List<PlantSenseData>>());

        PlantSenseData data = results.ShouldHaveSingleItem();
        data.Model.ShouldBe("PlantSense");
        data.Battery.ShouldBe(4);
        data.DeviceId.ShouldBe("3c84279d2150");
        data.Name.ShouldBe("aprikose");
        data.Test.ShouldBe(false);
        data.Snr.ShouldBe(10.25f);
    }
}