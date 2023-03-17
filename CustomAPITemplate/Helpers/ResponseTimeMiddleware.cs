namespace CustomAPITemplate.Helpers;

public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;
    public ResponseTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        context.Response.OnStarting(state =>
        {
            sw.Stop();
            context.Response.Headers.TryAdd("X-Response-Time", sw.ElapsedMilliseconds.ToString());
            return Task.CompletedTask;
        }, context);

        await _next(context);
    }
}