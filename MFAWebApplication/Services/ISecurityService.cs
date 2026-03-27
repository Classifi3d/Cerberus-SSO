namespace MFAWebApplication.Services;

public interface ISecurityService
{
    public string CreateJSONWebToken(Guid userId);
    public string GenerateEncodedMfaKey();
    public byte[]? GenerateQRCode(string encodedMfaKey, string userEmail);
    public bool CheckTotp(string MfaKey, string TotpCode);
    public string HashPassword(string plainPassword);
    public bool CheckPassword(string plainPassword, string hashPassword);
    public string HashSecret(string plainSecret);
    public bool CheckSecret(string plainSecret, string hashedSecret);


}
