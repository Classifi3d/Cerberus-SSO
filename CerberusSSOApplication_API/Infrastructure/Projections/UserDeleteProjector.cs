using Application.Abstraction;
using Application.DTOs;
using Application.Projections.Interfaces;
using Domain.Entities.User;
using MessagePack;

namespace Infrastructure.Projections;

public class UserDeleteProjector : IEventProjector
{
    public string EventType => nameof(UserDeleteEvent);
    private readonly IReadModelRepository<UserReadModel> _repository;
    private readonly UserMapper _mapper;

    public UserDeleteProjector(IReadModelRepository<UserReadModel> repository, UserMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<UserDeleteEvent>(payload, null, cancellationToken);
        if (userEvent == null) return;

        var readModel = _mapper.ToReadModel(userEvent);
        //var readModel = new UserReadModel
        //{
        //    Id = userEvent.Id.ToString(),
        //    ConcurrencyIndex = userEvent.ConcurrencyIndex
        //};

        await _repository.DeleteIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
