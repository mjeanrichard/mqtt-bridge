using FluentAssertions;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Input;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;
using System.Text;

namespace MqttBridge.Tests;

public class GasMeterTests
{
    [Test]
    public async Task Test()
    {
        string json = """
         {
            "address": 3,
            "id": "21072718",
            "manufacturer": 7910,
            "version": 54,
            "medium": 3,
            "access_no": 1,
            "status": "0",
            "signature": "0",
            "values": [
                {
                    "vif": 120,
                    "code": 31,
                    "scalar": 0,
                    "value_raw": 21072718,
                    "value_scaled": 2.1072718e7
                },
                {
                    "vif": 20,
                    "code": 2,
                    "scalar": -2,
                    "value_raw": 618981,
                    "value_scaled": 6189.81
                }
            ],
            "millis": 5117,
            "battery_mv": 2363,
            "vol_m3": 6189.81
        }
        """;

        JsonMessageSerializer<GasMeterMessage> jsonMessageSerializer = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        (object? message, Type messageType) = await jsonMessageSerializer.DeserializeAsync(stream, new MessageHeaderCollection(), MessageSerializationContext.Empty);

        message.Should().BeOfType<GasMeterMessage>();
        ((GasMeterMessage)message!).BatteryMilivolts.Should().Be(2363);
        ((GasMeterMessage)message).Values[0].ScaledValue.Should().Be(21072718);
    }

}