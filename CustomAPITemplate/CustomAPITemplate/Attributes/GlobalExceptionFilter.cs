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

        var supportId = Guid.NewGuid();

        context.Result = new ObjectResult($"Internal Server Error in {context.ActionDescriptor.DisplayName}, Support Id: {supportId}")
        {
            StatusCode = 500
        };

        Log.ForContext<GlobalExceptionFilter>().Fatal(context.Exception, "Error in {DisplayName}, Support Id: {SupportId}", context.ActionDescriptor.DisplayName, supportId);
    }
}
