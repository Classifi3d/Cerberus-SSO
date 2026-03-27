namespace MFAWebApplication.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string clientId);
    string GenerateRefreshToken();
}