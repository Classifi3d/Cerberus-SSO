using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using Application.DTOs;
using CSharpFunctionalExtensions;
using Domain.Entities.Client;

namespace Application.CommandsAndQueries.Clients;

public sealed record ExchangeTokenCommand(TokenRequestDTO tokenRequest) : ICommand<TokenResponseDTO>;

public sealed class ExchangeTokenCommandHandler : ICommandHandler<ExchangeTokenCommand, TokenResponseDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ITokenService _tokenService;
    private readonly ISecurityService _securityService;

    public ExchangeTokenCommandHandler(
        IUnitOfWork unitOfWork, 
        ICacheService cacheService, 
        ITokenService tokenService, 
        ISecurityService securityService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _tokenService = tokenService;
        _securityService = securityService;
    }

    public async Task<Result<TokenResponseDTO>> Handle(ExchangeTokenCommand request, CancellationToken cancellationToken)
    {
        var req = request.tokenRequest;

        if (req.GrantType != "authorization_code")
        {
            return Result.Failure<TokenResponseDTO>("Unsupported grant_type");
        }

        var client = await _unitOfWork.Repository<Client>()
           .GetByPropertyAsync(c => c.ClientId, req.ClientId, cancellationToken);

        if (client is null) {
            return Result.Failure<TokenResponseDTO>("Invalid client");
        }

        // Typed rather than dynamic: the cache deserializes to a JsonElement, and every
        // member access on it through `dynamic` threw at runtime instead of reading the
        // stored code.
        var cached = await _cacheService.GetAsync<AuthorizationCodeDTO>($"auth_code_{req.Code}");
        if (cached is null) {
            return Result.Failure<TokenResponseDTO>("Invalid or expired code");
        }

        if (cached.ClientId != req.ClientId)
        {
            return Result.Failure<TokenResponseDTO>("Invalid code for this client");
        }

        // A public client proves possession with PKCE; a confidential one with its
        // secret. Which applies is decided by how the client is registered, never by
        // what the caller chose to send.
        var isPublicClient = string.IsNullOrEmpty(client.ClientSecret);

        if (isPublicClient)
        {
            if (!Pkce.Verify(req.CodeVerifier, cached.CodeChallenge))
            {
                return Result.Failure<TokenResponseDTO>("Invalid code_verifier");
            }
        }
        else
        {
            if (!_securityService.CheckSecret(req.ClientSecret ?? string.Empty, client.ClientSecret))
            {
                return Result.Failure<TokenResponseDTO>("Invalid client credentials");
            }

            // Confidential clients may still use PKCE; honour the challenge if one was
            // registered with the code.
            if (!string.IsNullOrWhiteSpace(cached.CodeChallenge) &&
                !Pkce.Verify(req.CodeVerifier, cached.CodeChallenge))
            {
                return Result.Failure<TokenResponseDTO>("Invalid code_verifier");
            }
        }

        // RFC 6749 section 4.1.3: the redirect uri must match the one the code was
        // issued for.
        if (!string.IsNullOrWhiteSpace(cached.RedirectUri) &&
            !string.Equals(cached.RedirectUri, req.RedirectUri, StringComparison.Ordinal))
        {
            return Result.Failure<TokenResponseDTO>("redirect_uri does not match the authorization request");
        }

        if (!Guid.TryParse(cached.UserId, out var userId))
        {
            return Result.Failure<TokenResponseDTO>("The authorization code references an unusable user id");
        }

        // Name and email are stamped into the token so a relying party can show who is
        // signed in without a second round trip to the user endpoint.
        var user = await _unitOfWork.Repository<Domain.Entities.User.User>()
            .GetByIdAsync(userId, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(
            userId,
            req.ClientId,
            user?.Username,
            user?.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _cacheService.RemoveAsync($"auth_code_{req.Code}");

        return Result.Success(new TokenResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 900
        });
    }
}