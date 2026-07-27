using Application.Abstraction;
using Application.CommandsAndQueries.Clients;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// OAuth 2.0 endpoints.
/// </summary>
/// <remarks>
/// Every parameter is bound by its explicit wire name. OAuth uses snake_case
/// (client_id, response_type, code_challenge) while model binding matches on property
/// name, so binding the DTOs directly produced empty objects and every authorization
/// failed on the response_type check before reaching any real logic.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class OAuthController : Controller
{
    private readonly IMediator _mediator;

    public OAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("authorize")]
    public async Task<IActionResult> AuthorizeClientAsync(
        [FromQuery(Name = "client_id")] string? clientId,
        [FromQuery(Name = "redirect_uri")] string? redirectUri,
        [FromQuery(Name = "response_type")] string? responseType,
        [FromQuery(Name = "state")] string? state,
        [FromQuery(Name = "scope")] string? scope,
        [FromQuery(Name = "code_challenge")] string? codeChallenge,
        [FromQuery(Name = "code_challenge_method")] string? codeChallengeMethod,
        CancellationToken cancellationToken = default)
    {
        var request = new AuthorizationRequestDTO
        {
            ClientId = clientId ?? string.Empty,
            RedirectUri = redirectUri ?? string.Empty,
            ResponseType = responseType ?? string.Empty,
            State = state ?? string.Empty,
            Scope = scope,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod
        };

        var command = new AuthorizeClientCommand(request);
        var result = await _mediator.Send<AuthorizeClientCommand, AuthorizeClientResultDTO>(command, cancellationToken);

        if (result.IsFailure) {
            return BadRequest(result.Error);
        }

        return Redirect(result.Value.RedirectUrl);
    }

    [HttpPost]
    [Route("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> TokenAsync(
        [FromForm(Name = "grant_type")] string? grantType,
        [FromForm(Name = "code")] string? code,
        [FromForm(Name = "client_id")] string? clientId,
        [FromForm(Name = "redirect_uri")] string? redirectUri,
        [FromForm(Name = "client_secret")] string? clientSecret,
        [FromForm(Name = "code_verifier")] string? codeVerifier,
        CancellationToken cancellationToken = default)
    {
        var request = new TokenRequestDTO
        {
            GrantType = grantType ?? string.Empty,
            Code = code ?? string.Empty,
            ClientId = clientId ?? string.Empty,
            RedirectUri = redirectUri ?? string.Empty,
            ClientSecret = clientSecret,
            CodeVerifier = codeVerifier
        };

        var command = new ExchangeTokenCommand(request);
        var result = await _mediator.Send<ExchangeTokenCommand, TokenResponseDTO>(command, cancellationToken);

        if (result.IsFailure)
        {
            // The shape an OAuth client expects for a rejected grant (RFC 6749 5.2).
            return BadRequest(new
            {
                error = "invalid_grant",
                error_description = result.Error
            });
        }

        return Ok(new
        {
            access_token = result.Value.AccessToken,
            refresh_token = result.Value.RefreshToken,
            token_type = "Bearer",
            expires_in = result.Value.ExpiresIn
        });
    }

    [HttpPost]
    [Route("clients")]
    public async Task<IActionResult> CreateClientAsync([FromBody] CreateClientRequestDTO request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand(request);
        var result = await _mediator.Send<CreateClientCommand, Guid>(command, cancellationToken);

        if (result.IsFailure) {
            return BadRequest(result.Error);
        }

        return Ok(new
        {
            client_id = request.ClientId,
            client_secret = request.ClientSecret,
            id = result.Value
        });
    }
}
