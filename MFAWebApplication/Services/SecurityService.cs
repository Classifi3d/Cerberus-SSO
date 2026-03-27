using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using OtpNet;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MFAWebApplication.Services;

public class SecurityService : ISecurityService
{

    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityService> _logger;

    public SecurityService(
        IConfiguration configuration,
        ILogger<SecurityService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string CreateJSONWebToken(Guid userId)
    {
        var claims = new List<Claim>(){
            new Claim(ClaimTypes.NameIdentifier,userId.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }


    public string GenerateEncodedMfaKey()
    {
        // Generate Base32-encoded secret key
        var key = KeyGeneration.GenerateRandomKey(20);
        var secretKey = Base32Encoding.ToString(key);
        return secretKey;
    }

    public byte[]? GenerateQRCode(string encodedMfaKey, string userEmail)
    {
        // OTP URL
        string issuer = Uri.EscapeDataString("MFA-Security");
        string accountName = Uri.EscapeDataString(userEmail);
        string otpauthUrl = $"otpauth://totp/{issuer}:{accountName}?secret={encodedMfaKey}&issuer={issuer}&digits=6";

        // Generate QR code as PNG bytes
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }


    public bool CheckTotp(string MfaKey, string TotpCode)
    {
        _logger.LogDebug("Entered TOTP: {TotpCode} with {MfaKey}", TotpCode, MfaKey);

        // Decode Base32 secret key
        byte[] secretKeyBytes = Google.Authenticator.Base32Encoding.ToBytes(MfaKey);

        // Create TOTP instance with a 30-second time step (Google Authenticator standard)
        var totp = new Totp(secretKeyBytes, step: 30);

        // Generate expected OTP for the current time
        var expectedTotp = totp.ComputeTotp();
        _logger.LogDebug("Expected TOTP: {ExpectedTotp}", expectedTotp);

        bool isValid = totp.VerifyTotp(TotpCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        return isValid;
    }


    public string HashPassword(string plainPassword)
    {
        // Generate salt
        byte[] salt = new byte[16];
        new SecureRandom().NextBytes(salt);

        int costFactor = 12;
        return OpenBsdBCrypt.Generate(plainPassword.ToCharArray(), salt, costFactor);
    }

    public bool CheckPassword(string plainPassword, string hashedPassword)
    {
        return OpenBsdBCrypt.CheckPassword(hashedPassword, plainPassword.ToCharArray());
    }

    public string HashSecret(string plainSecret)
    {
        var inputBytes = Encoding.UTF8.GetBytes(plainSecret);
        var inputHash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(inputHash);
    }

    public bool CheckSecret(string plainSecret, string hashedSecret)
    {
        var inputBytes = Encoding.UTF8.GetBytes(plainSecret);
        var inputHash = SHA256.HashData(inputBytes);
        string hashed = Convert.ToHexString(inputHash);
        return hashedSecret.Equals(hashed);
    }
}
