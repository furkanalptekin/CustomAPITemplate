using CustomAPITemplate.DB.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAPITemplate.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TransactionAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var dbContext = context.HttpContext.RequestServices.GetService<AppDbContext>();
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        var executedContext = await next();

        if (executedContext.HttpContext.Response.StatusCode is >= 200 and <= 299)
        {
            await transaction.CommitAsync();
        }
    }
}