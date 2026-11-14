using Application.DTOs;

namespace Application.Abstraction.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string clientId, string? username = null, string? email = null);
    string GenerateRefreshToken();

    /// <summary>
    /// The public half of the signing key, for the jwks_uri. Resource servers fetch
    /// this to validate tokens; without it an RS256 token cannot be verified by anyone.
    /// </summary>
    JsonWebKeyDTO GetPublicSigningKey();
}
