using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Reflection;

namespace CustomAPITemplate.Core.Excel;

//TODO: handle errors
public class ExcelHelper
{
    private readonly Type _stringType = typeof(string);
    private readonly IDictionary<string, List<PropertyInfo>> _properties = new Dictionary<string, List<PropertyInfo>>();
    private readonly ExcelSheetData[] _excelSheetData;

    public ExcelHelper(params ExcelSheetData[] excelSheetData)
    {
        _excelSheetData = excelSheetData;
        FillPropertiesDict();
    }

    public byte[] Create()
    {
        using var memoryStream = new MemoryStream();
        using var workbook = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook);

        var workbookPart = workbook.AddWorkbookPart();
        OpenXmlWriter writer;

        var style = workbookPart.AddNewPart<WorkbookStylesPart>();
        style.Stylesheet = OpenXmlHelper.CreateStylesheet();

        var sheetPartIdList = new List<string>();

        foreach (var excelSheetData in _excelSheetData)
        {
            var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
            sheetPartIdList.Add(workbookPart.GetIdOfPart(sheetPart));

            writer = OpenXmlWriter.Create(sheetPart);

            WriteWorksheet(writer, excelSheetData);
            
            writer.Close();
        }

        writer = OpenXmlWriter.Create(workbookPart);

        WriteWorkbook(writer, sheetPartIdList);

        writer.Close();
        workbook.Close();

        return memoryStream.ToArray();
    }

    private void WriteWorkbook(OpenXmlWriter writer, List<string> sheetPartIdList)
    {
        writer.WriteStartElement(new Workbook());

        WriteSheets(writer, sheetPartIdList);

        writer.WriteEndElement();
    }

    private void WriteSheets(OpenXmlWriter writer, List<string> sheetPartIdList)
    {
        writer.WriteStartElement(new Sheets());

        for (int i = 0; i < sheetPartIdList.Count; i++)
        {
            writer.WriteElement(new Sheet
            {
                Id = sheetPartIdList[i],
                SheetId = (uint)(i + 1),
                Name = _excelSheetData[i].SheetName
            });
        }

        writer.WriteEndElement();
    }

    private void WriteWorksheet(OpenXmlWriter writer, ExcelSheetData excelSheetData)
    {
        writer.WriteStartElement(new Worksheet());

        var headerCells = WriteColumnsAndGetHeaderCells(writer, excelSheetData);

        WriteSheetData(writer, excelSheetData, headerCells);

        writer.WriteEndElement();
    }

    private void WriteSheetData(OpenXmlWriter writer, ExcelSheetData excelSheetData, List<Cell> headerCells)
    {
        writer.WriteStartElement(new SheetData());

        WriteHeaderRow(writer, headerCells);
        WriteContentRows(writer, excelSheetData);

        writer.WriteEndElement();
    }

    private void WriteContentRows(OpenXmlWriter writer, ExcelSheetData excelSheetData)
    {
        var rowCount = 0;
        foreach (var item in excelSheetData.Data)
        {
            if (++rowCount >= 1_048_575) // Excel Row Limit - 1 (Header)
            {
                break;
            }

            writer.WriteStartElement(new Row());

            foreach (var (type, value) in GetValues(excelSheetData, item))
            {
                writer.WriteElement(OpenXmlHelper.GetCell(type, value));
            }

            writer.WriteEndElement();
        }
    }

    private void WriteHeaderRow(OpenXmlWriter writer, List<Cell> headerCells)
    {
        writer.WriteStartElement(new Row());

        foreach (var item in headerCells)
        {
            writer.WriteElement(item);
        }

        writer.WriteEndElement();
    }

    private List<Cell> WriteColumnsAndGetHeaderCells(OpenXmlWriter writer, ExcelSheetData excelSheetData)
    {
        writer.WriteStartElement(new Columns());
        var headerCells = new List<Cell>();

        for (int i = 0; i < excelSheetData.HeaderProperties.Count; i++)
        {
            var cell = new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(excelSheetData.HeaderProperties[i].HeaderName),
                StyleIndex = 2 //Header Style
            };

            var column = new Column
            {
                Min = (uint)i + 1,
                Max = (uint)i + 1,
                Width = 28,
                CustomWidth = true
            };

            writer.WriteElement(column);
            headerCells.Add(cell);
        }

        writer.WriteEndElement();
        return headerCells;
    }

    private void FillPropertiesDict()
    {
        foreach (var item in _excelSheetData)
        {
            var headers = item.HeaderProperties.Select(x => x.PropertyName).ToList();
            var properties = item.Data
                .First()
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(x => headers.Contains(x.Name))
                .ToList();

            _properties.TryAdd(item.SheetName, properties);
        }
    }

    private IEnumerable<(Type type, object value)> GetValues(ExcelSheetData sheetData, object data)
    {
        foreach (var item in sheetData.HeaderProperties)
        {
            yield return GetValueAndTypeByPropertyName(data, item.PropertyName, sheetData.SheetName);
        }
    }

    private (Type type, object value) GetValueAndTypeByPropertyName(object data, string propertyName, string sheetName)
    {
        if (!_properties.TryGetValue(sheetName, out var properties))
        {
            return (_stringType, string.Empty);
        }

        var prop = properties.FirstOrDefault(x => x.Name == propertyName);
        if (prop == null)
        {
            return (_stringType, string.Empty);
        }

        var value = prop.GetValue(data, null);
        return (value != null ? value.GetType() : _stringType, value);
    }
}