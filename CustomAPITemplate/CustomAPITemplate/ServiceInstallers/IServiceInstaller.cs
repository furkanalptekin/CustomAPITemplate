namespace CustomAPITemplate.ServiceInstallers;

public interface IServiceInstaller
{
    void InstallService(IServiceCollection services, IConfiguration configuration);
}