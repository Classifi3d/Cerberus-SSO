namespace MFAWebApplication.Services;

public interface ISecurityService
{
    public string CreateToken(Guid userId);
    public string HashPassword(string plainPassword);
    public bool CheckPassword(string plainPassword, string hashPassword);
}
