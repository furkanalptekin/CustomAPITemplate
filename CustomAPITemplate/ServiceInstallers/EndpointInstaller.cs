namespace CustomAPITemplate.ServiceInstallers;

public class EndpointInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
    }
}