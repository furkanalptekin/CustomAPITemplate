using CustomAPITemplate.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAPITemplate.Attributes;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(x => x.Key, x => x.Value.Errors.Select(error => error.ErrorMessage))
                .ToList();

            var response = new Response();
            foreach (var error in errors)
            {
                foreach (var subError in error.Value)
                {
                    response.Results.Add(new()
                    {
                        Message = subError,
                        Severity = Severity.Error
                    });
                }
            }

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}