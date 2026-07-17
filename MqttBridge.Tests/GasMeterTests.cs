using MqttBridge.Models.Input;
using Shouldly;

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

        GasMeterMessage message = await TestDeserializer.DeserializeAsync<GasMeterMessage>(json);

        message.BatteryMillivolts.ShouldBe(2363);
        message.Values[0].ScaledValue.ShouldBe(21072718);
    }
}