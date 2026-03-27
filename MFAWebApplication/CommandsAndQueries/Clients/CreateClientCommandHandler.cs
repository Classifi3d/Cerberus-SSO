using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.DTOs;
using MFAWebApplication.Entities.Client;
using MFAWebApplication.Services;

namespace MFAWebApplication.CommandsAndQueries.Clients;
public sealed record CreateClientCommand(CreateClientRequestDTO clientRequest) : ICommand<Guid>;

internal sealed class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, Guid>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;
    private readonly ISecurityService _securityService;

    public CreateClientCommandHandler(
        UnitOfWork<WriteDbContext> unitOfWork,
        ISecurityService securityService)
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
    }

    public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var requestedClient = request.clientRequest;
        var existingClient = await _unitOfWork.Repository<Client>()
            .GetByPropertyAsync(c => c.ClientId, requestedClient.ClientId, cancellationToken);

        if (existingClient is not null) {
            return Result.Failure<Guid>("Client already exists!");
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            ClientId = requestedClient.ClientId,
            ClientSecret = _securityService.HashSecret(requestedClient.ClientSecret),
            RedirectUri = requestedClient.RedirectUri,
        };

        await _unitOfWork.Repository<Client>().AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.Id);
    }
}
