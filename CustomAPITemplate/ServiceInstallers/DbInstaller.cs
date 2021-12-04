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
            opt.UseNpgsql(configuration.GetConnectionString("NpgsqlConnection"));
        });

        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();
    }
}