namespace CustomAPITemplate.Extensions;

public static class HttpContextExtensions
{
    public static string GetIPAdress(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString();
    }

    public static string GetUserId(this HttpContext context)
    {
        if (context.User == null)
        {
            return null;
        }

        return context.User.Claims.Single(x => x.Type == "id").Value;
    }
}