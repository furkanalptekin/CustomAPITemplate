using CustomAPITemplate.ServiceInstallers;

namespace CustomAPITemplate.Extensions;

public static class ServiceInstallerExtension
{
    public static void InstallServices(this WebApplicationBuilder builder)
    {
        var type = typeof(IServiceInstaller);

        var installers = type.Assembly.ExportedTypes
            .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IServiceInstaller>()
            .ToList();

        foreach (var installer in installers)
        {
            installer.InstallService(builder.Services, builder.Configuration);
        }
    }
}