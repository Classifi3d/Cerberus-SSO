namespace MFAWebApplication.Services;

public interface ICacheService
{
    ValueTask<T> GetAsync<T>(string key);
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    ValueTask<bool> RemoveAsync(string key);
    ValueTask<bool> KeyExistsAsync(string key);
    bool TryGetValue<T>(string key, out T value);
}
