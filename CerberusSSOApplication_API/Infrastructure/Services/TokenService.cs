using Application.Abstraction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly RsaSecurityKey _privateKey;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        var rsa = RSA.Create(2048); // change with value
        _privateKey = new RsaSecurityKey(rsa)
        {
            KeyId = Guid.NewGuid().ToString()
        };
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateAccessToken(Guid userId, string clientId)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("client_id", clientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            _privateKey,
            SecurityAlgorithms.RsaSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],// e.g. https://localhost:5000
            audience: clientId,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }
}
