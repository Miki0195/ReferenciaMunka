using System.Text.Json;
using StackExchange.Redis;

namespace ELTE.Cinema.Cache;

internal class RedisCacheManager : ICacheManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private const string KeyPrefix = "cinema_cache:";

    public RedisCacheManager(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<Dictionary<string, T>> GetAllByPatternAsync<T>(string pattern)
    {
        var keys = await GetKeysAsync(pattern);
        var dict = new Dictionary<string, T>();

        foreach (var key in keys)
        {
            var value = await GetAsync<T>(key);
            if (value != null)
            {
                dict.Add(key, value);
            }
        }

        return dict;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var redisValue = await _database.StringGetAsync(AddPrefix(key));
        return redisValue.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(redisValue!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var jsonData = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(AddPrefix(key), jsonData, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(AddPrefix(key));
    }

    public async Task RemoveAllByPatternAsync(string pattern)
    {
        var keys = await GetKeysAsync(pattern);
        foreach (var key in keys)
        {
            await _database.KeyDeleteAsync(key);
        }
    }

    public async Task ClearAsync()
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        await server.FlushAllDatabasesAsync();
    }

    private Task<List<string>> GetKeysAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server
            .Keys(pattern: $"{KeyPrefix}*{pattern}*")
            .Select(key => key.ToString().Replace(KeyPrefix, ""))
            .ToList();
        return Task.FromResult(keys);
    }

    private static string AddPrefix(string key) 
        => $"{KeyPrefix}{key}";
}