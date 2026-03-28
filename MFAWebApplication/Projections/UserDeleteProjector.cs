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
    public string EventType => nameof(UserDeletedEvent);

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<UserDeletedEvent>(payload);
        if (userEvent == null) return;

        var readModel = new UserReadModel
        {
            Id = userEvent.Id.ToString(),
            ConcurrencyIndex = userEvent.ConcurrencyIndex
        };

        await _repository.DeleteIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
