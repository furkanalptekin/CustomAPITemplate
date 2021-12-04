using System.Reflection;
using CustomAPITemplate.DB.Repositories;

namespace CustomAPITemplate.ServiceInstallers
{
    public class RepositoryInstaller : IServiceInstaller
    {
        public void InstallService(IServiceCollection services, IConfiguration configuration)
        {
            var type = typeof(AppDbContext);

            var assembly = Assembly.GetAssembly(type);
            if (assembly == null)
            {
                return;
            }

            services.AddScoped(type);

            var repositoryInterfaces = assembly.ExportedTypes
                .Where(x => x.IsInterface && x.Name.EndsWith("Repository") && x.Name != "IRepository")
                .ToList();

            var repositoryClasses = assembly.ExportedTypes
                .Where(x => x.IsClass && x.Name.EndsWith("Repository") && x.Name != "Repository")
                .ToList();

            foreach (var @class in repositoryClasses)
            {
                var @interface = repositoryInterfaces.FirstOrDefault(x => x.Name == $"I{@class.Name}");
                services.AddScoped(@interface, @class);
            }
        }
    }
}