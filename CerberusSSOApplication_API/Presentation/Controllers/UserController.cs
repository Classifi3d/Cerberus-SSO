using Application.Abstraction;
using Application.CommandsAndQueries.Users;
using Application.DTOs;
using Domain.Entities.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("[controller]")]
//[Authorize]
public class UserController : Controller
{

    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    [Route("user-data")]
    [ActionName("GetUserById")]
    public async Task<IActionResult> GetUserDataAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized("Invalid token.");
        }

        var query = new GetUserProfileQuery(userId);
        var result = await _mediator.Query<GetUserProfileQuery, UserReadModel>(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("sign-up")]
    //[EnableRateLimiting("registerLimiter")]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserDTO userDto, CancellationToken cancellationToken)
    {
        if (userDto is null)
        {
            return BadRequest("User data is required.");
        }

        var result = await _mediator.Send(new CreateUserCommand(userDto), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();

    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    //[EnableRateLimiting("loginLimiter")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginDTO userLoginDTO, CancellationToken cancellationToken)
    {
        var query = new LoginUserQuery(userLoginDTO);
        var result = await _mediator.Query<LoginUserQuery, LoginSecurityDTO>(query, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized("Invalid credentials.");
        }

        var loginResult = result.Value;

        if (loginResult.RequiresMfa)
        {
            return Ok(new
            {
                message = "MFA required. Please verify using the 6-digit code.",
                challengeId = loginResult.ChallengeId
            });
        }

        // An OAuth login resolves to a redirect carrying the authorization code, not to
        // a token. Returning only `token` here dropped that redirect on the floor and
        // handed the caller a null, which made the authorization code flow impossible
        // to complete.
        if (!string.IsNullOrEmpty(loginResult.RedirectUrl))
        {
            return Ok(new { redirectUrl = loginResult.RedirectUrl });
        }

        return Ok(new { token = loginResult.Token });
    }

    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UserDTO userDto, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(userDto);
        var result = await _mediator.Send<UpdateUserCommand, UserDTO>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    [HttpDelete]
    [Route("{userId:guid}")]
    public async Task<IActionResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok();
    }


    [HttpPost]
    [Route("enable-mfa")]
    [ActionName("GenerateUserQRCode")]
    public async Task<IActionResult> EnableMfaAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized("Invalid token.");
        }

        var command = new EnableMfaOfUserCommand(userId);
        var result = await _mediator.Send<EnableMfaOfUserCommand, byte[]>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return File(result.Value, "image/png");

    }

    [HttpPost]
    [Route("disable-mfa")]
    [ActionName("DisableUserMfa")]
    public async Task<IActionResult> MfaDisableAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized("Invalid token.");
        }

        var command = new DisableMfaOfUserCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("verify-mfa")]
    [EnableRateLimiting("mfaLimiter")]
    public async Task<IActionResult> VerifyMfaAsync([FromBody] MfaVerificationDTO verificationDto, CancellationToken cancellationToken)
    {
        var query = new VerifyMfaOfUserQuery(verificationDto);
        var result = await _mediator.Query<VerifyMfaOfUserQuery, LoginSecurityDTO>(query, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        // Mirrors the login endpoint. This used to return the Result wrapper itself,
        // so callers received {value, isSuccess, isFailure, error} instead of the
        // payload, and the OAuth redirect was buried a level down.
        var verification = result.Value;

        if (!string.IsNullOrEmpty(verification.RedirectUrl))
        {
            return Ok(new { redirectUrl = verification.RedirectUrl });
        }

        return Ok(new { token = verification.Token });
    }

}

