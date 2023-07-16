using CustomAPITemplate.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAPITemplate.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class SanitizeAttribute : Attribute, IAsyncActionFilter
{
    public string EntityName { get; } = "entity";
    public string[] PropertiesToSanitize { get; } = null;

    private static readonly Type _stringType = typeof(string);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.ContainsKey(EntityName))
        {
            context.Result = new NotFoundResult();
            return;
        }

        var entity = context.ActionArguments[EntityName];
        var properties = entity
            .GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);

        if (properties == null || properties.Length == 0)
        {
            context.Result = new NotFoundResult();
            return;
        }

        foreach (var property in properties)
        {
            if (PropertiesToSanitize != null && !PropertiesToSanitize.Contains(property.Name))
            {
                continue;
            }

            if (property.PropertyType == _stringType)
            {
                var value = property.GetValue(entity);
                if (value != null)
                {
                    var sanitizedValue = value.ToString().Sanitize();
                    property.SetValue(entity, string.IsNullOrWhiteSpace(sanitizedValue) ? null : sanitizedValue);
                }
            }
        }

        await next();
    }
}
