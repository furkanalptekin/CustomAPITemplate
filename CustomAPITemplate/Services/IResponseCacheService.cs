namespace CustomAPITemplate.Services;

public interface IResponseCacheService
{
    Task SetCacheResponseAsync(string key, object response, TimeSpan ttl);

    Task<string> GetCacheResponseAsync(string key);

    Task RemoveCacheResponseAsync(string endpointName);
}