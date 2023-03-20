using Serilog;

namespace CustomAPITemplate.Helpers;

public static class SerilogRequestLoggingOptions
{
    public static IApplicationBuilder UseSerilogRequestLoggingWithOptions(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(opt =>
        {
            opt.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var userId = httpContext.User?.FindFirst("id")?.Value;
                diagnosticContext.Set("UserId", userId ?? "anonymous");
            };
        });

        return app;
    }
}
