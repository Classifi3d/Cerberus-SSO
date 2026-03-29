namespace MFAWebApplication.DTOs;

public class AuthorizeClientResultDTO
{
    public string RedirectUrl { get; set; } = string.Empty;
    public bool RequiresLogin { get; set; }
}