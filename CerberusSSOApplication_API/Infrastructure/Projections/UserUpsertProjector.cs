using Application.Abstraction;
using Application.DTOs;
using Application.Projections.Interfaces;
using Domain.Entities.User;
using MessagePack;

namespace Infrastructure.Projections;

public class UserUpsertProjector : IEventProjector
{
    public string EventType => nameof(UserUpsertEvent);
    private readonly IReadModelRepository<UserReadModel> _repository;
    private readonly UserMapper _mapper;

    public UserUpsertProjector(IReadModelRepository<UserReadModel> repository, UserMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<UserUpsertEvent>(payload, null, cancellationToken);
        if (userEvent == null) return;

        var readModel = _mapper.ToReadModel(userEvent);

        await _repository.UpsertIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
