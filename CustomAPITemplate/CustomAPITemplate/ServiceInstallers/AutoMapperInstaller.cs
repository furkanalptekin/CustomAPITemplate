using CustomAPITemplate.Contract.V1;

namespace CustomAPITemplate.ServiceInstallers;

public class AutoMapperInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(AutoMapperProfiles));
    }
}