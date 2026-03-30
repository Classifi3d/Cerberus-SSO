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

    public AuthorizeClientCommandHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
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

        var requestId = Guid.NewGuid().ToString();

        await _cacheService.SetAsync(
            $"oauth_request_{requestId}",
            authRequest,
            TimeSpan.FromMinutes(5)
        );

        var redirectUrl = $"/api/auth/login?requestId={requestId}";

        return Result.Success(
            new AuthorizeClientResultDTO
            {
                RedirectUrl = redirectUrl,
            }
        );
    }
}
