using MqttBridge.Models.Input;
using Shouldly;

namespace MqttBridge.Tests;

/// <summary>
/// Regression tests for the deserialization the MQTT endpoints perform. The devices publish plain JSON
/// without Silverback's <c>x-message-type</c> header; deserialization must succeed against the configured
/// model type anyway (a missing header previously threw "Message type header (x-message-type) not found").
/// </summary>
public class EndpointDeserializationTests
{
    [Test]
    public async Task Deserialize_EnvSensorInfo_WithoutMessageTypeHeader_CamelCase()
    {
        // The info endpoint is configured with camelCase property naming.
        string json = """
                      {
                          "timestamp": 1690228102,
                          "name": "Bodenheizung",
                          "mac": "34:94:54:e6:fe:ac",
                          "ip": "192.168.5.28",
                          "rssi": 213,
                          "fwVersion": 53
                      }
                      """;

        EnvSensorInfoMessage message = await TestDeserializer.DeserializeAsync<EnvSensorInfoMessage>(json, useCamelCase: true);

        message.Name.ShouldBe("Bodenheizung");
        message.Ip.ShouldBe("192.168.5.28");
        message.Rssi.ShouldBe(213);
        message.FwVersion.ShouldBe(53);
    }

    [Test]
    public async Task Deserialize_FroniusDaily_WithoutMessageTypeHeader()
    {
        string json = """
                      {
                          "Date": "2023-09-10T00:00:00",
                          "Data": [ { "Seconds": 41700, "Produced": 584.38 } ]
                      }
                      """;

        FroniusDailyMessage message = await TestDeserializer.DeserializeAsync<FroniusDailyMessage>(json);

        message.Data.ShouldHaveSingleItem().Seconds.ShouldBe(41700);
    }
}
