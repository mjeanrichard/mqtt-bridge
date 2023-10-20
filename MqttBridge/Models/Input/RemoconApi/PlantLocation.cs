using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record PlantLocation(
    [property: JsonPropertyName("country")]
    string CountryCode,
    [property: JsonPropertyName("addr")] string Address,
    [property: JsonPropertyName("cityName")]
    string CityName,
    [property: JsonPropertyName("city")] long CityId,
    [property: JsonPropertyName("adm1Name")]
    string Admin1Name,
    [property: JsonPropertyName("adm1")] long Admin1Id,
    [property: JsonPropertyName("adm2Name")]
    string Admin2Name,
    [property: JsonPropertyName("adm2")] long Admin2Id,
    [property: JsonPropertyName("postalCode")]
    string PostalCode,
    [property: JsonPropertyName("lat")] double Latitude,
    [property: JsonPropertyName("lng")] double Longitude,
    [property: JsonPropertyName("radius")] double Radius);