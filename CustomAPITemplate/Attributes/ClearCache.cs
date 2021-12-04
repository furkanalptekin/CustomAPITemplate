using CustomAPITemplate.Core.Configuration;
using CustomAPITemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAPITemplate.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ClearCache : Attribute, IAsyncActionFilter
{
    private Type _objectResult;
    public ClearCache(Type objectResult)
    {
        _objectResult = objectResult;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var redisCacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSettings>();
        if (!redisCacheSettings.IsEnabled)
        {
            await next();
            return;
        }

        var executedContext = await next();
        if (executedContext.Result.GetType() != _objectResult)
        {
            return;
        }

        var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

        var routeValues = context.RouteData.Values;
        var controllerName = routeValues.ContainsKey("controller") ? routeValues["controller"].ToString() : null;

        if (string.IsNullOrEmpty(controllerName))
        {
            return;
        }

        await cacheService.RemoveCacheResponseAsync(controllerName);
    }
}