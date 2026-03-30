namespace Application.Abstraction;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    public void AddOutboxEvent(object domainEvent);
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default);
    public void Dispose();
}
