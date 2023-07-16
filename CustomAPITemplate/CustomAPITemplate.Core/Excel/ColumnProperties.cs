namespace CustomAPITemplate.Core.Excel;

public class ColumnProperties
{
    public required string HeaderName { get; init; }
    public required string PropertyName { get; init; }
    public string Format { get; init; }
}