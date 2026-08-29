using System.Text.Json.Serialization;

namespace Application.DTOs;

/// <summary>
/// A public RSA key in JWK form (RFC 7517), as published at the jwks_uri.
///
/// The property names are the exact wire names from the spec, so they are set
/// explicitly rather than left to a naming policy.
/// </summary>
public class JsonWebKeyDTO
{
    [JsonPropertyName("kty")]
    public string KeyType { get; set; } = "RSA";

    [JsonPropertyName("use")]
    public string Use { get; set; } = "sig";

    [JsonPropertyName("alg")]
    public string Algorithm { get; set; } = "RS256";

    [JsonPropertyName("kid")]
    public string KeyId { get; set; } = string.Empty;

    /// <summary>Base64url modulus.</summary>
    [JsonPropertyName("n")]
    public string Modulus { get; set; } = string.Empty;

    /// <summary>Base64url public exponent.</summary>
    [JsonPropertyName("e")]
    public string Exponent { get; set; } = string.Empty;
}
