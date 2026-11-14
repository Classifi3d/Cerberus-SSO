using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using Application.DTOs;
using CSharpFunctionalExtensions;
using Domain.Entities.Client;

namespace Application.CommandsAndQueries.Clients;

public sealed record AuthorizeClientCommand(AuthorizationRequestDTO AuthorizationRequest) : ICommand<AuthorizeClientResultDTO>;
public sealed class AuthorizeClientCommandHandler : ICommandHandler<AuthorizeClientCommand, AuthorizeClientResultDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IOAuthSettings _settings;

    public AuthorizeClientCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IOAuthSettings settings)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _settings = settings;
    }

    public async Task<Result<AuthorizeClientResultDTO>> Handle(AuthorizeClientCommand request, CancellationToken cancellationToken)
    {
        var authRequest = request.AuthorizationRequest;

        if (authRequest.ResponseType != "code")
        {
            return Result.Failure<AuthorizeClientResultDTO>("Unsupported response_type");
        }

        var client = await _unitOfWork.Repository<Client>().GetByPropertyAsync(
            c => c.ClientId, authRequest.ClientId, cancellationToken);

        if (client is null)
        {
            return Result.Failure<AuthorizeClientResultDTO>("Invalid client");
        }

        if (client.RedirectUri != authRequest.RedirectUri)
        {
            return Result.Failure<AuthorizeClientResultDTO>("Invalid redirect_uri");
        }

        // A client registered without a secret is public - a browser app - and has no
        // way to authenticate at the token endpoint except PKCE. Letting one through
        // without a challenge would make a stolen authorization code enough to get a
        // token.
        var isPublicClient = string.IsNullOrEmpty(client.ClientSecret);

        if (isPublicClient && string.IsNullOrWhiteSpace(authRequest.CodeChallenge))
        {
            return Result.Failure<AuthorizeClientResultDTO>(
                "code_challenge is required for public clients");
        }

        if (!string.IsNullOrWhiteSpace(authRequest.CodeChallenge) &&
            !string.Equals(authRequest.CodeChallengeMethod, Pkce.S256, StringComparison.Ordinal))
        {
            return Result.Failure<AuthorizeClientResultDTO>(
                "Unsupported code_challenge_method; only S256 is accepted");
        }

        var requestId = Guid.NewGuid().ToString();

        await _cacheService.SetAsync(
            $"oauth_request_{requestId}",
            authRequest,
            TimeSpan.FromMinutes(5)
        );

        // Absolute, and pointing at the login UI rather than at this API. The previous
        // relative "/api/auth/login" resolved against the API's own origin, where no
        // such route exists, so the flow died at its first redirect.
        var separator = _settings.LoginUrl.Contains('?') ? '&' : '?';
        var redirectUrl = $"{_settings.LoginUrl}{separator}requestId={requestId}";

        return Result.Success(
            new AuthorizeClientResultDTO
            {
                RedirectUrl = redirectUrl,
                RequiresLogin = true,
            }
        );
    }
}
