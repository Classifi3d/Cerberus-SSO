using Application.Abstraction;
using Application.Projections.Interfaces;
using Domain.Entities.User;
using MessagePack;

namespace Infrastructure.Projections;
public class UserUpsertProjector : IEventProjector
{

    private readonly IReadModelRepository<UserReadModel> _repository;

    public UserUpsertProjector(IReadModelRepository<UserReadModel> repository)
    {
        _repository = repository;
    }
    public string EventType => nameof(UserUpsertEvent);

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<UserUpsertEvent>(payload, null, cancellationToken);
        if (userEvent == null) return;

        // Implement mapper
        var readModel = new UserReadModel
        {
            Id = userEvent.Id.ToString(),
            Email = userEvent.Email,
            Username = userEvent.Username,
            Password = userEvent.Password,
            IsMfaEnabled = userEvent.IsMfaEnabled,
            ConcurrencyIndex = userEvent.ConcurrencyIndex
        };

        await _repository.UpsertIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
