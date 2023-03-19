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
    private readonly ExcelSheetData[] _excelSheetDatas;
    
    private OpenXmlWriter writer;
    private Dictionary<string, uint> styleIndexDict;

    public ExcelHelper(params ExcelSheetData[] excelSheetDatas)
    {
        _excelSheetDatas = excelSheetDatas;
        FillPropertiesDict();
    }

    public byte[] Create()
    {
        using var memoryStream = new MemoryStream();
        using var workbook = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook);

        var workbookPart = workbook.AddWorkbookPart();

        var style = workbookPart.AddNewPart<WorkbookStylesPart>();
        (style.Stylesheet, styleIndexDict) = OpenXmlHelper.CreateStylesheet(_excelSheetDatas);

        var sheetPartIdList = new List<string>();

        foreach (var excelSheetData in _excelSheetDatas)
        {
            var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
            sheetPartIdList.Add(workbookPart.GetIdOfPart(sheetPart));

            writer = OpenXmlWriter.Create(sheetPart);

            WriteWorksheet(excelSheetData);
            
            writer.Close();
        }

        writer = OpenXmlWriter.Create(workbookPart);

        WriteWorkbook(sheetPartIdList);

        writer.Close();
        workbook.Close();

        return memoryStream.ToArray();
    }

    private void WriteWorkbook(List<string> sheetPartIdList)
    {
        writer.WriteStartElement(new Workbook());

        WriteSheets(sheetPartIdList);

        writer.WriteEndElement();
    }

    private void WriteSheets(List<string> sheetPartIdList)
    {
        writer.WriteStartElement(new Sheets());

        for (int i = 0; i < sheetPartIdList.Count; i++)
        {
            writer.WriteElement(new Sheet
            {
                Id = sheetPartIdList[i],
                SheetId = (uint)(i + 1),
                Name = _excelSheetDatas[i].SheetName
            });
        }

        writer.WriteEndElement();
    }

    private void WriteWorksheet(ExcelSheetData excelSheetData)
    {
        writer.WriteStartElement(new Worksheet());

        WriteColumns(excelSheetData);
        WriteSheetData(excelSheetData);

        writer.WriteEndElement();
    }

    private void WriteSheetData(ExcelSheetData excelSheetData)
    {
        writer.WriteStartElement(new SheetData());

        WriteHeaderRow(excelSheetData);
        WriteContentRows(excelSheetData);

        writer.WriteEndElement();
    }

    private void WriteContentRows(ExcelSheetData excelSheetData)
    {
        var rowCount = 0;
        foreach (var item in excelSheetData.Data)
        {
            if (++rowCount >= 1_048_575) // Excel Row Limit - 1 (Header)
            {
                break;
            }

            writer.WriteStartElement(new Row());

            foreach (var properties in GetValues(excelSheetData, item))
            {
                writer.WriteElement(OpenXmlHelper.GetCell(properties, styleIndexDict));
            }

            writer.WriteEndElement();
        }
    }

    private void WriteHeaderRow(ExcelSheetData excelSheetData)
    {
        writer.WriteStartElement(new Row());

        foreach (var item in excelSheetData.ColumnProperties)
        {
            var cell = new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(item.HeaderName),
                StyleIndex = 2 //Header Style
            };

            writer.WriteElement(cell);
        }

        writer.WriteEndElement();
    }

    private void WriteColumns(ExcelSheetData excelSheetData)
    {
        writer.WriteStartElement(new Columns());

        for (int i = 0; i < excelSheetData.ColumnProperties.Count; i++)
        {
            var column = new Column
            {
                Min = (uint)i + 1,
                Max = (uint)i + 1,
                Width = 28,
                CustomWidth = true
            };

            writer.WriteElement(column);
        }

        writer.WriteEndElement();
    }

    private void FillPropertiesDict()
    {
        foreach (var item in _excelSheetDatas)
        {
            var headers = item.ColumnProperties.Select(x => x.PropertyName).ToList();
            var properties = item.Data
                .First()
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(x => headers.Contains(x.Name))
                .ToList();

            _properties.TryAdd(item.SheetName, properties);
        }
    }

    private IEnumerable<CellProperties> GetValues(ExcelSheetData sheetData, object data)
    {
        foreach (var item in sheetData.ColumnProperties)
        {
            yield return GetValueAndTypeByPropertyName(data, item, sheetData.SheetName);
        }
    }

    private CellProperties GetValueAndTypeByPropertyName(object data, ColumnProperties columnProperties, string sheetName)
    {
        var cellProperties = new CellProperties
        {
            Type = _stringType,
            Value = string.Empty,
            Format = columnProperties.Format,
        };

        if (!_properties.TryGetValue(sheetName, out var properties))
        {
            return cellProperties;
        }

        var prop = properties.FirstOrDefault(x => x.Name == columnProperties.PropertyName);
        if (prop == null)
        {
            return cellProperties;
        }

        cellProperties.Value = prop.GetValue(data, null);
        if (cellProperties.Value != null)
        {
            cellProperties.Type = cellProperties.Value.GetType();
        }

        return cellProperties;
    }
}