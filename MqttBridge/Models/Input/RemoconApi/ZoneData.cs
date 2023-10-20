using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record ZoneData(
    [property: JsonPropertyName("mode")] ValueAndOptions Mode,
    [property: JsonPropertyName("heatingOn")]
    bool IsHeatingActive,
    [property: JsonPropertyName("coolingOn")]
    bool IsCoolingActive,
    [property: JsonPropertyName("hasRoomSens")]
    bool HasRoomSensor,
    [property: JsonPropertyName("chComfTemp")]
    NumericValueAndRange ChComfortTemp,
    [property: JsonPropertyName("chRedTemp")]
    NumericValueAndRange ChReducedTemp,
    [property: JsonPropertyName("coolComfTemp")]
    NumericValueAndRange CoolComfortTemp,
    [property: JsonPropertyName("coolRedTemp")]
    NumericValueAndRange CoolReducedTemp,
    [property: JsonPropertyName("roomTemp")]
    decimal RoomTemp,
    [property: JsonPropertyName("heatOrCoolReq")]
    bool HeatOrCoolRequest,
    [property: JsonPropertyName("chProtTemp")]
    decimal ChProtectionTemp,
    [property: JsonPropertyName("coolProtTemp")]
    decimal CoolProtectionTemp,
    [property: JsonPropertyName("holidays")]
    List<Holiday> Holidays,
    [property: JsonPropertyName("useReducedOperationModeOnHoliday")]
    bool UseReducedOperationModeOnHoliday,
    [property: JsonPropertyName("desiredRoomTemp")]
    decimal DesiredRoomTemp,
    [property: JsonPropertyName("roomTempError")]
    bool RoomTempError);