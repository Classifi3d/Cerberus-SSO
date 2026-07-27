namespace Application.DTOs;

public class TokenRequestDTO
{
    public string GrantType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Confidential clients only. Public clients authenticate with PKCE instead.</summary>
    public string? ClientSecret { get; set; }

    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>The PKCE verifier whose SHA-256 must match the stored challenge.</summary>
    public string? CodeVerifier { get; set; }
}

