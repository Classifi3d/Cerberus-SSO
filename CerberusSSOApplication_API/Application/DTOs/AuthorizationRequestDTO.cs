namespace Application.DTOs;

public class AuthorizationRequestDTO
{
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Scope { get; set; }

    /// <summary>
    /// PKCE challenge (RFC 7636). Required for public clients, which cannot keep a
    /// client secret; it is what proves the token request comes from whoever started
    /// the authorization.
    /// </summary>
    public string? CodeChallenge { get; set; }

    /// <summary>Only S256 is accepted. `plain` adds no protection over a leaked code.</summary>
    public string? CodeChallengeMethod { get; set; }
}
