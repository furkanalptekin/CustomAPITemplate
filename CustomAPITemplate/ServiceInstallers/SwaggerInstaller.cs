﻿using Microsoft.OpenApi.Models;

namespace CustomAPITemplate.ServiceInstallers;

public class SwaggerInstaller : IServiceInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(opt =>
        {
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}