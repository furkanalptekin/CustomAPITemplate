using System.Reflection;
using System.Text;
using CustomAPITemplate.Core.Configuration;
using CustomAPITemplate.Core.Extensions;
using CustomAPITemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace CustomAPITemplate.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _ttl;

    public CacheAttribute(int ttl = 900)
    {
        _ttl = ttl;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var redisCacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSettings>();
        if (!redisCacheSettings.IsEnabled)
        {
            await next();
            return;
        }

        var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

        var cacheKey = GenerateCacheKeyFromRequest(context);
        var cachedResponse = await cacheService.GetCacheResponseAsync(cacheKey);

        if (!string.IsNullOrWhiteSpace(cachedResponse))
        {
            Log.ForContext<CacheAttribute>().Information("Returning from cache: {CacheKey}", cacheKey);
            context.Result = new ContentResult
            {
                Content = cachedResponse,
                ContentType = "application/json; charset=utf-8",
                StatusCode = 200
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Result is OkObjectResult okObjectResult)
        {
            await cacheService.SetCacheResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_ttl));
        }
    }

    private static string GenerateCacheKeyFromRequest(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var stringBuilder = new StringBuilder($"{request.Method}|{request.Path.Value?.ToLowerEN() ?? string.Empty}");

        if (request.Method == "GET")
        {
            return GenerateGetKey(context, stringBuilder);
        }

        return GeneratePostKey(context, stringBuilder);
    }

    private static string GenerateGetKey(ActionExecutingContext context, StringBuilder builder)
    {
        foreach (var (key, value) in context.HttpContext.Request.Query.OrderBy(x => x.Key))
        {
            builder.Append($"|{key}-{value}");
        }

        return builder.ToString();
    }

    private static string GeneratePostKey(ActionExecutingContext context, StringBuilder builder)
    {
        if (!context.ActionArguments.TryGetValue("entity", out var requestData))
        {
            return builder.ToString();
        }

        var type = requestData.GetType();

        if (type == null)
        {
            return builder.ToString();
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        foreach (var property in properties?.OrderBy(x => x.Name) ?? Enumerable.Empty<PropertyInfo>())
        {
            if (!property.CanRead || !property.CanWrite)
                continue;
            
            builder.Append($"|{property.Name}-{property.GetValue(requestData, null)}");
        }

        return builder.ToString();
    }
}