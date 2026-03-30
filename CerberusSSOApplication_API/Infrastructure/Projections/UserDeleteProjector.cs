using Application.Abstraction;
using Application.Projections.Interfaces;
using Domain.Entities.User;
using MessagePack;

namespace Infrastructure.Projections;

public class UserDeleteProjector : IEventProjector
{

    private readonly IReadModelRepository<UserReadModel> _repository;
    public UserDeleteProjector(IReadModelRepository<UserReadModel> repository)
    {
        _repository = repository;
    }
    public string EventType => nameof(UserDeleteEvent);

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<UserDeleteEvent>(payload, null, cancellationToken);
        if (userEvent == null) return;

        var readModel = new UserReadModel
        {
            Id = userEvent.Id.ToString(),
            ConcurrencyIndex = userEvent.ConcurrencyIndex
        };

        await _repository.DeleteIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
