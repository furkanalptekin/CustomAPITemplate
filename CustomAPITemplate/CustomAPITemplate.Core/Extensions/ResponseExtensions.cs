namespace CustomAPITemplate.Core;

public static class ResponseExtensions
{
    public static Response<T> AddError<T>(this Response<T> response, string message)
    {
        return AddResult(response, message, Severity.Error);
    }

    public static Response AddError(this Response response, string message)
    {
        return AddResult(response, message, Severity.Error);
    }

    public static Response<T> AddWarning<T>(this Response<T> response, string message)
    {
        return AddResult(response, message, Severity.Warning);
    }

    public static Response AddWarning(this Response response, string message)
    {
        return AddResult(response, message, Severity.Warning);
    }

    public static Response<T> AddInfo<T>(this Response<T> response, string message)
    {
        return AddResult(response, message, Severity.Info);
    }

    public static Response AddInfo(this Response response, string message)
    {
        return AddResult(response, message, Severity.Info);
    }

    private static Response AddResult(Response response, string message, Severity severity)
    {
        response.Results.Add(new Result
        {
            Message = message,
            Severity = severity,
        });

        return response;
    }

    private static Response<T> AddResult<T>(Response<T> response, string message, Severity severity)
    {
        response.Results.Add(new Result
        {
            Message = message,
            Severity = severity,
        });

        return response;
    }
}