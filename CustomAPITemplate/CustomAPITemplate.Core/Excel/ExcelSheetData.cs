namespace CustomAPITemplate.Core.Excel;

public class ExcelSheetData
{
    public required List<ColumnProperties> ColumnProperties { get; init; }
    public required string SheetName { get; init; }
    public required List<object> Data { get; init; }
}