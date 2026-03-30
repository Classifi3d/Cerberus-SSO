using Application.Abstraction;
using Application.CommandsAndQueries.Clients;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

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
    public async Task<IActionResult> AuthorizeClientAsync([FromQuery] AuthorizationRequestDTO request,
        CancellationToken cancellationToken)
    {

        var command = new AuthorizeClientCommand(request);
        var result = await _mediator.Send<AuthorizeClientCommand, AuthorizeClientResultDTO>(command, cancellationToken);

        if (result.IsFailure) {
            return BadRequest(result.Error);
        }

        return Redirect(result.Value.RedirectUrl);
    }

    [HttpPost]
    [Route("token")]
    public async Task<IActionResult> TokenAsync([FromForm] TokenRequestDTO request, CancellationToken cancellationToken)
    {
        var command = new ExchangeTokenCommand(request);
        var result = await _mediator.Send<ExchangeTokenCommand, TokenResponseDTO>(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
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
