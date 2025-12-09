using AuthenticationWebApplication.Enteties;
using MessagePack;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Entities;
using MFAWebApplication.Projections.Interfaces;

namespace MFAWebApplication.Projections;

public class UserDeleteProjector : IEventProjector
{

    private readonly IReadModelRepository<UserReadModel> _repository;
    public UserDeleteProjector(IReadModelRepository<UserReadModel> repository)
    {
        _repository = repository;
    }
    public string EventType => nameof(UserDeleteProjector);

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<User>(payload);
        if (userEvent == null) return;

        var readModel = new UserReadModel
        {
            Id = userEvent.Id.ToString(),
            Email = userEvent.Email,
            Username = userEvent.Username,
            Password = userEvent.Password,
            IsMfaEnabled = userEvent.IsMfaEnabled,
            ConcurrencyIndex = userEvent.ConcurrencyIndex
        };

        await _repository.DeleteIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
