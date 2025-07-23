using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ELTE.Cinema.Cache;

public enum CacheType {InMemory, Redis}

public static class DependencyInjection
{
    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration? config = null)
    {
        var redisConnectionString = config?.GetConnectionString("Redis");

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheManager, InMemoryCacheManager>();
        }
        else
        {
                
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<ICacheManager, RedisCacheManager>();
        }

        return services;
    }

}