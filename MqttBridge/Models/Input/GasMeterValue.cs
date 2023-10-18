using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input;

/**
 * {
 *    "address": 3,
 *    "id": "21072718",
 *    "manufacturer": 7910,
 *    "version": 54,
 *    "medium": 3,
 *    "access_no": 1,
 *    "status": "0",
 *    "signature": "0",
 *    "values": [
 *        {
 *            "vif": 120,
 *            "code": 31,
 *            "scalar": 0,
 *            "value_raw": 21072718,
 *            "value_scaled": 2.1072718e7
 *        },
 *        {
 *            "vif": 20,
 *            "code": 2,
 *            "scalar": -2,
 *            "value_raw": 618981,
 *            "value_scaled": 6189.81
 *        }
 *    ],
 *    "millis": 5117,
 *    "battery_mv": 2363,
 *    "vol_m3": 6189.81
 * }
 */
public class GasMeterValue
{
    [JsonPropertyName("vif")]
    public int Vif { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("scalar")]
    public int Scalar { get; set; }

    [JsonPropertyName("value_raw")]
    public int RawValue { get; set; }

    [JsonPropertyName("value_scaled")]
    public double ScaledValue { get; set; }
}