using System.Linq;
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
        AddPendingOutboxEvents(cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesWithConcurrencyRetryAsync(CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;

        for (var attempt = 1; ; attempt++)
        {
            AddPendingOutboxEvents(cancellationToken);

            try
            {
                return await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
            {
                foreach (var entry in ex.Entries)
                {
                    // Capture the caller's pending changes before reload wipes them out.
                    var pendingChanges = entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                    // Refresh the stored values (including the concurrency token) so the
                    // retry's UPDATE matches the current row.
                    await entry.ReloadAsync(cancellationToken);

                    // Re-apply the caller's changes on top of the fresh stored values.
                    foreach (var change in pendingChanges)
                    {
                        entry.Property(change.Key).CurrentValue = change.Value;
                        entry.Property(change.Key).IsModified = true;
                    }
                }
            }
        }
    }

    private void AddPendingOutboxEvents(CancellationToken cancellationToken)
    {
        if (_pendingOutboxEvents.Count == 0)
        {
            return;
        }

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

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
