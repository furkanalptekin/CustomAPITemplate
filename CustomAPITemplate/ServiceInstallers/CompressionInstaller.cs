using Microsoft.AspNetCore.ResponseCompression;

namespace CustomAPITemplate.ServiceInstallers;

public class CompressionInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });
    }
}