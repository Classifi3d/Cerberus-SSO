namespace MFAWebApplication.DTOs;

public class LoginSecurityDTO
{
    public string? Token { get; set; }
    public bool RequiresMfa { get; set; }
    public string? ChallengeId { get; set; }
    public string? RedirectUrl { get; set; }
}
