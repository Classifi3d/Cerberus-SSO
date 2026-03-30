namespace Application.DTOs;

public class AuthorizationRequestDTO
{
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Scope { get; set; }
}
