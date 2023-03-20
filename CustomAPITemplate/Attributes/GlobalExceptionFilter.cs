using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace CustomAPITemplate.Attributes;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        context.Result = new ObjectResult($"Internal Server Error in {context.ActionDescriptor.DisplayName}")
        {
            StatusCode = 500
        };

        Log.ForContext<GlobalExceptionFilter>().Fatal(context.Exception, "Error in {DisplayName}", context.ActionDescriptor.DisplayName);
    }
}
