using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record RemotePlantFeatures(
    [property: JsonPropertyName("zones")] List<Zone> Zones,
    [property: JsonPropertyName("solar")] bool HasSolar,
    [property: JsonPropertyName("convBoiler")]
    bool HasConventionalBoiler,
    [property: JsonPropertyName("commBoiler")]
    bool HasCommercialBoiler,
    [property: JsonPropertyName("hpSys")] bool IsHpSystem,
    [property: JsonPropertyName("hybridSys")]
    bool IsHybridSystem,
    [property: JsonPropertyName("cascadeSys")]
    bool IsCascadeSystem,
    [property: JsonPropertyName("dhwProgSupported")]
    bool? IsDhwProgSupported,
    [property: JsonPropertyName("virtualZones")]
    bool HasVirtualZones,
    [property: JsonPropertyName("hasVmc")] bool HasVmc,
    [property: JsonPropertyName("extendedTimeProg")]
    bool HasExtendedTimeProg,
    [property: JsonPropertyName("hasBoiler")]
    bool HasBoiler,
    [property: JsonPropertyName("pilotSupported")]
    bool IsPilotSupported,
    [property: JsonPropertyName("isVmcR2")]
    bool IsVmcR2,
    [property: JsonPropertyName("isEvo2")] bool IsEvo2,
    [property: JsonPropertyName("dhwHidden")]
    bool? IsDhwHidden,
    [property: JsonPropertyName("dhwBoilerPresent")]
    bool? IsDhwBoilerPresent,
    [property: JsonPropertyName("dhwModeChangeable")]
    bool? IsDhwModeChangeable,
    [property: JsonPropertyName("hvInputOff")]
    bool? IsHvInputOff,
    [property: JsonPropertyName("autoThermoReg")]
    bool HasAutomaticThermoregulation,
    [property: JsonPropertyName("hasMetering")]
    bool HasMetering,
    [property: JsonPropertyName("weatherProvider")]
    WeatherProviders WeatherProvider,
    [property: JsonPropertyName("hasFireplace")]
    bool HasFireplace,
    [property: JsonPropertyName("hasSlp")] bool? HasSlp,
    [property: JsonPropertyName("hasEm20")]
    bool HasEm20,
    [property: JsonPropertyName("hasTwoCoolingTemp")]
    bool HasTwoCoolingTemp,
    [property: JsonPropertyName("bmsActive")]
    bool? BmsActive,
    [property: JsonPropertyName("hpCascadeSys")]
    bool IsHpCascadeSystem,
    [property: JsonPropertyName("hpCascadeConfig")]
    int? HpCascadeConfig,
    [property: JsonPropertyName("bufferTimeProgAvailable")]
    bool BufferTimeProgAvailable,
    [property: JsonPropertyName("distinctHeatCoolSetpoints")]
    bool HasDistinctHeatCoolSetpoints,
    [property: JsonPropertyName("hasZoneNames")]
    bool HasZoneNames,
    [property: JsonPropertyName("hydraulicScheme")]
    int? HydraulicScheme,
    [property: JsonPropertyName("preHeatingSupported")]
    bool PreHeatingSupported,
    [property: JsonPropertyName("hasGahp")]
    bool HasGahp,
    [property: JsonPropertyName("hasDhwTimeProgTemperatures")]
    TriStateBoolean? HasDhwTimeProgTemperatures,
    [property: JsonPropertyName("zigbeeActive")]
    bool ZigbeeActive);