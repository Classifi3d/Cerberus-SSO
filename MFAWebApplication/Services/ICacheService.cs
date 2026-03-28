namespace MFAWebApplication.Services;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    bool TryGetValue<T>(string key, out T value);
}
