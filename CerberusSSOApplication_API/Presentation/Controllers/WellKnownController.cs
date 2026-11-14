using Application.Abstraction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// OpenID Connect discovery surface.
///
/// Resource servers - the Synapse Analyzer API among them - are configured with this
/// service as their authority. They fetch the document below, follow jwks_uri to the
/// public signing key, and validate tokens with it. Without these two endpoints an
/// RS256 token issued here cannot be verified by anybody.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route(".well-known")]
public class WellKnownController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IOAuthSettings _settings;

    public WellKnownController(ITokenService tokenService, IOAuthSettings settings)
    {
        _tokenService = tokenService;
        _settings = settings;
    }

    [HttpGet("openid-configuration")]
    public IActionResult GetConfiguration()
    {
        var issuer = _settings.Issuer;

        return Ok(new
        {
            issuer,
            authorization_endpoint = $"{issuer}/OAuth/authorize",
            token_endpoint = $"{issuer}/OAuth/token",
            jwks_uri = $"{issuer}/.well-known/jwks.json",
            response_types_supported = new[] { "code" },
            grant_types_supported = new[] { "authorization_code" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" },
            token_endpoint_auth_methods_supported = new[] { "client_secret_post", "none" },
            code_challenge_methods_supported = new[] { "S256" },
            scopes_supported = new[] { "openid", "profile", "email" },
            claims_supported = new[] { "sub", "client_id", "preferred_username", "email", "jti" }
        });
    }

    [HttpGet("jwks.json")]
    public IActionResult GetJwks()
    {
        return Ok(new { keys = new[] { _tokenService.GetPublicSigningKey() } });
    }
}
