using AutoMapper;
using MessagePack;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Entities;
using MFAWebApplication.Projections.Interfaces;

namespace MFAWebApplication.Projections;
public class UserUpsertProjector : IEventProjector
{

    private readonly IReadModelRepository<UserReadModel> _repository;
    private readonly IMapper _mapper;

    public UserUpsertProjector(IReadModelRepository<UserReadModel> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public string EventType => nameof(UserUpsertEvent);

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var userEvent = MessagePackSerializer.Deserialize<UserUpsertEvent>(payload);
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
        //readModel = _mapper.Map<UserReadModel>(userEvent);

        await _repository.UpsertIfMatchingConcurrencyAsync(readModel, cancellationToken);
    }
}
