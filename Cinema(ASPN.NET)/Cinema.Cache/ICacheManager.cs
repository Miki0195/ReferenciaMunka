namespace ELTE.Cinema.Cache;

public interface ICacheManager
{
    Task<Dictionary<string, T>> GetAllByPatternAsync<T>(string pattern);
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveAllByPatternAsync(string pattern);
    Task ClearAsync();
}