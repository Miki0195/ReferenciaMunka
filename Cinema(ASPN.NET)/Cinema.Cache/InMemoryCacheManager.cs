using Microsoft.Extensions.Caching.Memory;

namespace ELTE.Cinema.Cache;

internal class InMemoryCacheManager: ICacheManager
{
    private readonly List<string> _keys;
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheManager(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _keys = [];
    }
    
    public Task<Dictionary<string, T>> GetAllByPatternAsync<T>(string pattern)
    {
        var matchingKeys = _keys.Where(key => key.Contains(pattern));
        var dict = new Dictionary<string, T>();

        foreach (var key in matchingKeys)
        {
            var value = _memoryCache.Get<T>(key);
            if (value != null)
            {
                dict.Add(key, value);
            }
        }

        return Task.FromResult(dict);
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_memoryCache.Get<T>(key));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _ = expiration.HasValue ? _memoryCache.Set(key, value, expiration.Value) : _memoryCache.Set(key, value);
       
        if (!_keys.Contains(key))
        {
            _keys.Add(key);
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _keys.Remove(key);
        
        return Task.CompletedTask;
    }

    public Task RemoveAllByPatternAsync(string pattern)
    {
        var matchingKeys = _keys.Where(key => key.Contains(pattern));
        foreach (var key in matchingKeys)
        {
            _memoryCache.Remove(key);
        }
        
        _keys.Clear();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        foreach (var key in _keys)
        {
            _memoryCache.Remove(key); // Remove each key
        }

        _keys.Clear();
        return Task.CompletedTask;
    }
}