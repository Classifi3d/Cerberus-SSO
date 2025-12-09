using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
//using Org.BouncyCastle.Crypto.Generators;
//using Org.BouncyCastle.Security;

namespace MFAWebApplication.Services;

public class SecurityService : ISecurityService
{

    private readonly IConfiguration _configuration;

    public SecurityService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(Guid userId)
    {
        List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.NameIdentifier,userId.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    // Old Implementation SHA256 implementation
    public string HashPassword(string inputString)
    {
        var inputBytes = Encoding.UTF8.GetBytes(inputString);
        var inputHash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(inputHash);
    }

    public bool CheckPassword(string plainPassword, string hashPassword)
    {
        var inputBytes = Encoding.UTF8.GetBytes(plainPassword);
        var inputHash = SHA256.HashData(inputBytes);
        string hashed =  Convert.ToHexString(inputHash);
        return hashPassword.Equals(hashed);
    }

    //public string HashPassword(string plainPassword)
    //{
    //    int costFactor = 12;
    //    return BCrypt.Net.BCrypt.HashPassword(plainPassword, costFactor);
    //}

    //public bool CheckPassword(string plainPassword, string hashPassword)
    //{
    //    return BCrypt.Net.BCrypt.Verify(plainPassword, hashPassword);
    //}
}
