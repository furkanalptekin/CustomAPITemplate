using CustomAPITemplate.Core.Configuration;
using CustomAPITemplate.Services;
using StackExchange.Redis;

namespace CustomAPITemplate.ServiceInstallers;

public class RedisInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        var redisCacheSettings = new RedisCacheSettings();
        configuration.GetSection(nameof(RedisCacheSettings)).Bind(redisCacheSettings);
        services.AddSingleton(redisCacheSettings);

        if (redisCacheSettings.IsEnabled)
        {
            //services.AddStackExchangeRedisCache(opt => opt.Configuration = redisCacheSettings.ConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(redisCacheSettings.ConnectionString));
            services.AddSingleton<IResponseCacheService, ResponseCacheService>();
        }
    }
}