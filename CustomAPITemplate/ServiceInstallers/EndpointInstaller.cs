using CustomAPITemplate.Attributes;
using CustomAPITemplate.Contract.V1;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.ServiceInstallers;

public class EndpointInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(opt =>
        {
            opt.Filters.Add<ValidationFilter>();
        })
            .AddFluentValidation(conf =>
            {
                conf.RegisterValidatorsFromAssemblyContaining<IRequestBase>();
            });
        services.AddEndpointsApiExplorer();

        services.Configure<ApiBehaviorOptions>(opt =>
        {
            opt.SuppressModelStateInvalidFilter = true;
        });
    }
}