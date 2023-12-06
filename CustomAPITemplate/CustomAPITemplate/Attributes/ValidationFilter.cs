using System.Reflection;
using CustomAPITemplate.Contract;
using CustomAPITemplate.Core;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAPITemplate.Attributes;

public class ValidationFilter : IAsyncActionFilter
{
    private static readonly Type s_validator = typeof(IValidator<>);
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var request in context.ActionArguments.Select(x => x.Value))
        {
            var type = request.GetType();
            if (!type.IsClass)
            {
                continue;
            }

            Sanitize(request);

            var result = await Validate(context, request);
            if (result != null)
            {
                context.Result = result;
                return;
            }
        }

        await next();
    }

    private static void Sanitize(object request)
    {
        if (request is ISanitizable sanitizable && sanitizable.PropertiesToSanitize != null && sanitizable.PropertiesToSanitize.Length > 0)
        {
            var properties = sanitizable
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(x => sanitizable.PropertiesToSanitize.Contains(x.Name));

            foreach (var property in properties ?? [])
            {
                var value = property.GetValue(sanitizable);
                if (value == null)
                {
                    continue;
                }
                property.SetValue(sanitizable, value.ToString().Sanitize());
            }
        }
    }

    private static async Task<BadRequestObjectResult> Validate(ActionExecutingContext context, object request)
    {
        var service = context.HttpContext.RequestServices.GetService(s_validator.MakeGenericType(request.GetType()));
        if (service is IValidator validator)
        {
            var result = await validator.ValidateAsync(new ValidationContext<object>(request), context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                var response = result.ToDictionary().Aggregate(new Response(), (res, dict) =>
                {
                    res.Results.AddRange(dict.Value.Select(x => Result.Error(x)));
                    return res;
                });

                return new BadRequestObjectResult(response);
            }
        }

        return default;
    }
}