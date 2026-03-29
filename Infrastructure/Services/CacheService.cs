using Application.Abstraction.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IConnectionMultiplexer redis,
        ILogger<CacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async ValueTask<T> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (!value.HasValue)
            return default!;

        return JsonSerializer.Deserialize<T>((string)value!)!;
    }

    public async ValueTask SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serializedValue, (Expiration)expiry!);
    }

    public async ValueTask<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async ValueTask<bool> KeyExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        value = default!;
        try
        {
            var redisValue = _database.StringGet(key);

            if (!redisValue.HasValue)
            {
                return false;
            }

            value = JsonSerializer.Deserialize<T>((string)redisValue!)!;
            return true;
        }
        catch(Exception exception)
        {
            _logger.LogError("Error in CacheService in TryGetValue " +
                "for key: {Key}. {Exception}", key, exception);
            return false;
        }
    }
}
