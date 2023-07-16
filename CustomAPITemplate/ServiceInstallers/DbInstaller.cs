using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories;
using CustomAPITemplate.Services;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.ServiceInstallers;

public class DbInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            //TODO: Secure connection string
#if (DatabaseProvider == "SqlServer")
            opt.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
#endif
#if (DatabaseProvider == "PostgreSQL")
            opt.UseNpgsql(configuration.GetConnectionString("NpgsqlConnection"));
#endif
        });

        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();
    }
}