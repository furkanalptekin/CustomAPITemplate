namespace CustomAPITemplate.Core;

public class Result
{
    public string Message { get; set; }
    public Severity Severity { get; set; }

    public static Result Error(string message)
    {
        return new Result
        {
            Message = message,
            Severity = Severity.Error,
        };
    }

    public static Result Info(string message)
    {
        return new Result
        {
            Message = message,
            Severity = Severity.Info,
        };
    }

    public static Result Warning(string message)
    {
        return new Result
        {
            Message = message,
            Severity = Severity.Warning,
        };
    }
}