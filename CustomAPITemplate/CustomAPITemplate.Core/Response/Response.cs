namespace CustomAPITemplate.Core;

public class Response<T> : Response
{
    public T Value { get; set; }

    public static implicit operator Response<T>(T value)
    {
        var response = new Response<T>
        {
            Value = value
        };

        return response;
    }

    public static implicit operator Response<T>(Result result)
    {
        var response = new Response<T>();
        response.Results.Add(result);
        return response;
    }

    public static implicit operator Response<T>(List<Result> results)
    {
        var response = new Response<T>();
        response.Results.AddRange(results);
        return response;
    }
}

public class Response
{
    private List<Result> _results;
    public List<Result> Results
    {
        get
        {
            _results ??= [];
            return _results;
        }
        set
        {
            _results = value;
        }
    }

    public bool Success
    {
        get
        {
            return !Results.Any(x => x.Severity == Severity.Error);
        }
    }

    public static implicit operator Response(Result result)
    {
        var response = new Response();
        response.Results.Add(result);
        return response;
    }

    public static implicit operator Response(List<Result> results)
    {
        var response = new Response();
        response.Results.AddRange(results);
        return response;
    }
}