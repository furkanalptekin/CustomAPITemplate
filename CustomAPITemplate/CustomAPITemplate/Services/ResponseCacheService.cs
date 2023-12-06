using CustomAPITemplate.Core;
using StackExchange.Redis;

namespace CustomAPITemplate.Services;

public class ResponseCacheService(IConnectionMultiplexer _connectionMultiplexer) : IResponseCacheService
{
    public async Task<string> GetCacheResponseAsync(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task RemoveCacheResponseAsync(string endpointName)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var endpoint = _connectionMultiplexer.GetEndPoints().First();
        if (db == null || endpoint == null)
        {
            return;
        }

        var keys = _connectionMultiplexer.GetServer(endpoint)?.Keys(pattern: $"*{endpointName}*")?.ToList();

        foreach (var key in keys ?? Enumerable.Empty<RedisKey>())
        {
            await db.KeyDeleteAsync(key);
        }
    }

    public async Task SetCacheResponseAsync(string key, object response, TimeSpan ttl)
    {
        if (response == null)
        {
            return;
        }

        var serializedResponse = response.ToJson();

        var db = _connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, serializedResponse, ttl);
    }
}