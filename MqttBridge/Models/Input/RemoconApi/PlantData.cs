using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record PlantData(
    [property: JsonPropertyName("gw")] string GatewayId,
    [property: JsonPropertyName("dhwMode")]
    ValueAndOptions DhwMode,
    [property: JsonPropertyName("dhwComfTemp")]
    NumericValueAndRange DhwComfortTemp,
    [property: JsonPropertyName("dhwReduTemp")]
    NumericValueAndRange DhwReducedTemp,
    [property: JsonPropertyName("hasOutTemp")]
    bool HasOutsideTempProbe,
    [property: JsonPropertyName("outTemp")]
    decimal OutsideTemp,
    [property: JsonPropertyName("flame")] bool FlameSensor,
    [property: JsonPropertyName("dhwEnabled")]
    bool DhwEnabled,
    [property: JsonPropertyName("hpOn")] bool HeatPumpOn,
    [property: JsonPropertyName("dhwTemp")]
    decimal DhwStorageTemp,
    [property: JsonPropertyName("hasDhwTemp")]
    bool HasDhwStorageProbe,
    [property: JsonPropertyName("zones")] IDictionary<int, ZoneData> ZoneData,
    [property: JsonPropertyName("dhwProgReadOnly")]
    bool IsDhwProgReadOnly,
    [property: JsonPropertyName("dhwStorageTempError")]
    bool DhwStorageTempError,
    [property: JsonPropertyName("outsideTempError")]
    bool OutsideTempError);