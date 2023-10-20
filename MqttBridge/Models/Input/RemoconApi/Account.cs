using System.Text.Json.Serialization;

namespace MqttBridge.Models.Input.RemoconApi;

public record Account(
    [property: JsonPropertyName("FirstName")]
    string? FirstName,
    [property: JsonPropertyName("LastName")]
    string? LastName,
    [property: JsonPropertyName("Email")] string? Email,
    [property: JsonPropertyName("Role")] string? Role,
    [property: JsonPropertyName("CountryCode")]
    string? CountryCode,
    [property: JsonPropertyName("AcceptTermsAndCondsOfGdpr")]
    bool AcceptedTerms
);