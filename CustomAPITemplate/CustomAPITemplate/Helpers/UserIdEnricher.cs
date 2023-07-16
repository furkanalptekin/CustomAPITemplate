using Serilog.Core;
using Serilog.Events;

namespace CustomAPITemplate.Helpers;

public class UserIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserIdEnricher() 
        : this(new HttpContextAccessor())
    {

    }

    public UserIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst("id")?.Value;
        logEvent.AddOrUpdateProperty(new LogEventProperty("UserId", new ScalarValue(userId ?? "anonymous")));
    }
}