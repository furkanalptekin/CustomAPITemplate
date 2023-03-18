namespace CustomAPITemplate.Core.Excel;

public class ExcelSheetData
{
    public required List<HeaderProperties> HeaderProperties { get; init; }
    public required string SheetName { get; init; }
    public required List<object> Data { get; init; }
}