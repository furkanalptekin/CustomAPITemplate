namespace CustomAPITemplate.Core;

public class Response<T> : Response
{
    public T Value { get; set; }
}

public class Response
{
    private List<Result> _results;
    public List<Result> Results
    {
        get
        {
            if (_results == null)
            {
                _results = new List<Result>();
            }
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
}