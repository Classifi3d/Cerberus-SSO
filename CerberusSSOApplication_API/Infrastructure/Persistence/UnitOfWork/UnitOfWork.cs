using Application.Abstraction;
using Infrastructure.Outbox;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.UnitOfWork;

public sealed class UnitOfWork<TContext> : IUnitOfWork, IDisposable
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
    private readonly List<object> _pendingOutboxEvents = new List<object>();
    public UnitOfWork(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (_repositories.TryGetValue(type, out var repo))
        {
            return (IRepository<TEntity>)repo;
        }

        var newRepo = new Repository<TEntity>(_dbContext);
        _repositories[type] = newRepo;
        return newRepo;
    }

    public void AddOutboxEvent(object domainEvent)
    {
        _pendingOutboxEvents.Add(domainEvent);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_pendingOutboxEvents.Count > 0)
        {
            foreach (var outboxEvent in _pendingOutboxEvents)
            {
                var message = new OutboxMessage
                {
                    Type = outboxEvent.GetType().Name,
                    Payload = MessagePack.MessagePackSerializer.Serialize(outboxEvent, null, cancellationToken)
                };

                _dbContext.Set<OutboxMessage>().Add(message);
            }

            _pendingOutboxEvents.Clear();
        }

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
