namespace Application.DTOs;

/// <summary>
/// What is cached against an issued authorization code. The challenge and redirect
/// uri are carried here so the token exchange can verify them without trusting
/// anything the client re-sends.
/// </summary>
public class AuthorizationCodeDTO
{
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? CodeChallenge { get; set; }
    public string? RedirectUri { get; set; }
}