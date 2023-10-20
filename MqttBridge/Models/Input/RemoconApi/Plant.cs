using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record Plant
(
    [property: JsonPropertyName("umSys")] MeasurementSystems MeasurementSystem,
    [property: JsonPropertyName("gwId")] string GatewayId,
    [property: JsonPropertyName("gwSerial")]
    string GatewaySerial,
    [property: JsonPropertyName("plantName")]
    string Name,
    [property: JsonPropertyName("location")]
    PlantLocation? Location,
    [property: JsonPropertyName("gwSysType")]
    PlantSystemTypes SystemType,
    [property: JsonPropertyName("utcOffset")]
    int UtcOffset,
    [property: JsonPropertyName("gwLink")] PlantConnectionTypes LinkType,
    [property: JsonPropertyName("gwFwVer")]
    string GatewayFirmwareVersion,
    [property: JsonPropertyName("controlledByGuest")]
    bool ControlledByGuest,
    [property: JsonPropertyName("mqttApiVersion")]
    int MqttApiVersion);