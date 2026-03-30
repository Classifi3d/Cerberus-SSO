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

        var validSecret = _securityService.CheckSecret(req.ClientSecret, client.ClientSecret);
        if (!validSecret)
        {
            return Result.Failure<TokenResponseDTO>("Invalid client credentials");
        }

        var cached = await _cacheService.GetAsync<dynamic>($"auth_code_{req.Code}");
        if (cached is null) {
            return Result.Failure<TokenResponseDTO>("Invalid or expired code");
        }

        if (cached.ClientId != req.ClientId)
        {
            return Result.Failure<TokenResponseDTO>("Invalid code for this client");
        }
        
        // OPTIONAL_TODO: validate redirect_uri if you stored it

        Guid userId = Guid.Parse((string)cached.UserId);
        var accessToken = _tokenService.GenerateAccessToken(userId, req.ClientId);
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