using System.Security.Cryptography;
using System.Text;

namespace Application.CommandsAndQueries.Clients;

/// <summary>Proof Key for Code Exchange verification (RFC 7636).</summary>
public static class Pkce
{
    public const string S256 = "S256";

    /// <summary>
    /// True when <paramref name="codeVerifier"/> hashes to <paramref name="expectedChallenge"/>.
    /// </summary>
    public static bool Verify(string? codeVerifier, string? expectedChallenge)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier) || string.IsNullOrWhiteSpace(expectedChallenge))
            return false;

        // RFC 7636 section 4.1. A verifier outside this range is malformed, and
        // accepting a short one would weaken the whole exchange.
        if (codeVerifier.Length is < 43 or > 128)
            return false;

        var actual = ComputeChallenge(codeVerifier);

        // Fixed-time comparison: the challenge is a secret-derived value, and a
        // length-or-content-dependent comparison leaks it a byte at a time.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(actual),
            Encoding.ASCII.GetBytes(expectedChallenge));
    }

    /// <summary>BASE64URL(SHA256(ASCII(verifier))), unpadded.</summary>
    public static string ComputeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));

        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
