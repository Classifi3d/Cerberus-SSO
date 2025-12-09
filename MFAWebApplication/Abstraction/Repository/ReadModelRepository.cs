using MFAWebApplication.Context;
using MFAWebApplication.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace MFAWebApplication.Abstraction.Repository;

public class ReadModelRepository<TEntity> : IReadModelRepository<TEntity> where TEntity : ReadModel
{
    private readonly IMongoCollection<TEntity> _collection;

    public ReadModelRepository(ReadDbContext readDbContext)
    {
        _collection = readDbContext.GetCollection<TEntity>(typeof(TEntity).Name);
    }
    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(FilterDefinition<TEntity>.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id.ToString());
        return await _collection
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByPropertyAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector,
        TProperty value,
        CancellationToken cancellationToken = default)
    {
        var member = (propertySelector.Body as MemberExpression)!;
        var fieldName = member.Member.Name;

        var filter = Builders<TEntity>.Filter.Eq(fieldName, value);
        return await _collection
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<bool> UpsertIfNewerConcurrencyAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var id = GetEntityId(entity);
            var filter = Builders<TEntity>.Filter.And(
                Builders<TEntity>.Filter.Eq("_id", id),
                Builders<TEntity>.Filter.Lt("concurrencyIndex", entity.ConcurrencyIndex)
            );

            var update = new List<UpdateDefinition<TEntity>>();

            var props = typeof(TEntity).GetProperties();
            foreach (var prop in typeof(TEntity).GetProperties())
            {
                var name = prop.Name;

                if (name.Equals("_id", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = prop.GetValue(entity);
                update.Add(Builders<TEntity>.Update.Set(name, value));
            }
            update.Add(Builders<TEntity>.Update.SetOnInsert("_id", id));

            var updateDefinition = Builders<TEntity>.Update.Combine(update);

            var options = new UpdateOptions { IsUpsert = true };
            var result = await _collection.UpdateOneAsync(filter, updateDefinition, options, cancellationToken);

            try
            {
                return result.IsAcknowledged && (result.UpsertedId != null || result.ModifiedCount > 0);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

    }

    public async Task<bool> DeleteIfMatchingConcurrencyAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var id = GetEntityId(entity);

        var filter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq("_id", id),
            Builders<TEntity>.Filter.Lt("concurrencyIndex", entity.ConcurrencyIndex)
        );


        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    private static object? GetEntityId(TEntity entity)
    {
        var prop = typeof(TEntity).GetProperty("Id");
        return prop?.GetValue(entity);
    }

}

