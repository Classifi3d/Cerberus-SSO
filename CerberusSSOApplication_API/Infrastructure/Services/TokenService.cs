using Application.Abstraction.Services;
using Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Issues the OAuth access tokens.
///
/// The signing key is loaded once and kept. It used to be generated inside the
/// constructor, so every resolution of this scoped service produced a different
/// keypair and no token could be validated afterwards - not even by this service.
/// The key now comes from configuration, or from a file created on first run so a
/// local stack works with no setup while still surviving a restart.
/// </summary>
public class TokenService : ITokenService
{
    private readonly RsaSecurityKey _signingKey;
    private readonly string _keyId;
    private readonly IOAuthSettings _settings;

    public TokenService(
        IConfiguration configuration,
        IOAuthSettings settings,
        ILogger<TokenService> logger)
    {
        _settings = settings;

        var rsa = LoadOrCreateKey(configuration, logger);

        // The kid is derived from the key itself (RFC 7638), so the value in a token
        // header always matches the entry published in the JWKS.
        _keyId = ComputeThumbprint(rsa);
        _signingKey = new RsaSecurityKey(rsa) { KeyId = _keyId };
    }

    public string GenerateAccessToken(
        Guid userId,
        string clientId,
        string? username = null,
        string? email = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("client_id", clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(username))
            claims.Add(new Claim("preferred_username", username));

        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: clientId,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    public JsonWebKeyDTO GetPublicSigningKey()
    {
        var parameters = _signingKey.Rsa.ExportParameters(includePrivateParameters: false);

        return new JsonWebKeyDTO
        {
            KeyId = _keyId,
            Modulus = Base64UrlEncoder.Encode(parameters.Modulus),
            Exponent = Base64UrlEncoder.Encode(parameters.Exponent)
        };
    }

    // ---- key management ----------------------------------------------------

    private static RSA LoadOrCreateKey(IConfiguration configuration, ILogger logger)
    {
        var inlinePem = configuration["JWT:PrivateKeyPem"];

        if (!string.IsNullOrWhiteSpace(inlinePem))
        {
            var configured = RSA.Create();
            configured.ImportFromPem(inlinePem);
            logger.LogInformation("Loaded the JWT signing key from configuration.");
            return configured;
        }

        var path = configuration["JWT:SigningKeyPath"];

        if (string.IsNullOrWhiteSpace(path))
            path = Path.Combine(AppContext.BaseDirectory, "keys", "cerberus-signing-key.pem");

        if (File.Exists(path))
        {
            var stored = RSA.Create();
            stored.ImportFromPem(File.ReadAllText(path));
            logger.LogInformation("Loaded the JWT signing key from {Path}.", path);
            return stored;
        }

        var created = RSA.Create(2048);

        try
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, created.ExportPkcs8PrivateKeyPem());

            if (!OperatingSystem.IsWindows())
            {
                // The private key must not be world-readable.
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            logger.LogWarning(
                "No JWT signing key was configured; generated one and saved it to {Path}. " +
                "Set JWT:PrivateKeyPem or JWT:SigningKeyPath for anything beyond local development.",
                path);
        }
        catch (Exception ex)
        {
            // An in-memory key still signs, but tokens stop validating across restarts
            // and across instances, so this has to be loud.
            logger.LogError(
                ex,
                "Could not persist the generated JWT signing key to {Path}. Tokens issued by " +
                "this instance will stop validating when it restarts.",
                path);
        }

        return created;
    }

    /// <summary>RFC 7638 JWK thumbprint of the public key.</summary>
    private static string ComputeThumbprint(RSA rsa)
    {
        var parameters = rsa.ExportParameters(includePrivateParameters: false);

        // Exact member order and no whitespace are required by the spec.
        var canonical = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["e"] = Base64UrlEncoder.Encode(parameters.Exponent),
            ["kty"] = "RSA",
            ["n"] = Base64UrlEncoder.Encode(parameters.Modulus)
        });

        return Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
