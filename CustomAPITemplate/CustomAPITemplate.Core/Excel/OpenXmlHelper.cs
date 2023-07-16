using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CustomAPITemplate.Core.Excel;

internal static partial class OpenXmlHelper
{
    private const string FONT_NAME = "Calibri";
    private const int FONT_SIZE = 11;
    private const string FONT_COLOR = "000000";

    internal static Cell GetCell(CellProperties properties, Dictionary<string, uint> styleIndexDict)
    {
        var cellValue = GetCellValue(properties.TypeAsString, properties.Value, out var isNull, out var hasNewLine);

        return new Cell
        {
            DataType = GetDataType(properties.TypeAsString),
            CellValue = cellValue,
            StyleIndex = GetCellStyleIndex(properties, styleIndexDict, isNull, hasNewLine),
        };
    }

    private static CellValues GetDataType(string type)
    {
        switch (type)
        {
            case Constants.Types.DateTime:
            case Constants.Types.DateTimeOffset:
                return CellValues.Date;

            case Constants.Types.Byte:
            case Constants.Types.SByte:
            case Constants.Types.Short:
            case Constants.Types.UShort:
            case Constants.Types.Int:
            case Constants.Types.UInt:
            case Constants.Types.Long:
            case Constants.Types.ULong:
            case Constants.Types.Float:
            case Constants.Types.Double:
            case Constants.Types.Decimal:
                return CellValues.Number;

            case Constants.Types.Bool:
                return CellValues.Boolean;

            default:
                return CellValues.String;
        }
    }

    private static uint GetCellStyleIndex(CellProperties properties, Dictionary<string, uint> styleIndexDict, bool isNull, bool hasNewLine)
    {
        if (isNull)
        {
            return 3U; // Cell Format 3 -> Yellow Fill
        }

        if (!string.IsNullOrWhiteSpace(properties.Format) && styleIndexDict.TryGetValue(properties.Format, out var index))
        {
            return index;
        }

        if (styleIndexDict.TryGetValue(properties.TypeAsString, out var typeIndex))
        {
            return typeIndex;
        }

        if (hasNewLine)
        {
            return 1U; // TextWrap: true
        }

        return 0U; // Default
    }

    private static CellValue GetCellValue(string type, object data, out bool isNull, out bool hasNewLine)
    {
        isNull = false;
        hasNewLine = false;

        if (data == null)
        {
            isNull = true;
            return new CellValue(null);
        }

        switch (type)
        {
            case Constants.Types.DateTime:
                return new CellValue((DateTime)data);

            case Constants.Types.DateTimeOffset:
                return new CellValue((DateTimeOffset)data);

            case Constants.Types.Byte:
            case Constants.Types.SByte:
            case Constants.Types.Short:
            case Constants.Types.UShort:
            case Constants.Types.Int:
                return new CellValue((int)data);

            case Constants.Types.Float:
            case Constants.Types.Double:
                return new CellValue((double)data);

            case Constants.Types.Decimal:
                return new CellValue((decimal)data);

            case Constants.Types.Bool:
                return new CellValue((bool)data);
        }
        
        var value = data.ToString();
        value = RemoveInvalidXMLChars(value);
        hasNewLine = value.Contains('\n');
        return new CellValue(value);
    }

    internal static (Stylesheet StyleSheet, Dictionary<string, uint> StyleIndexDict) CreateStylesheet(ExcelSheetData[] excelSheetDatas)
    {
        var (numeringFormats, formatsDict) = GetNumberingFormats(excelSheetDatas);
        var (cellFormats, styleIndexDict) = GetCellFormats(formatsDict);

        return (new Stylesheet
        {
            Fonts = GetFonts(),
            Fills = GetFills(),
            Borders = GetBorders(),
            CellFormats = cellFormats,
            NumberingFormats = numeringFormats
        }, styleIndexDict);
    }

    private static Fonts GetFonts()
    {
        var defaultFont = new Font
        {
            FontSize = new FontSize { Val = FONT_SIZE },
            FontName = new FontName { Val = FONT_NAME },
            Color = new Color { Rgb = FONT_COLOR }
        };

        var boldFont = new Font
        {
            FontSize = new FontSize { Val = FONT_SIZE },
            FontName = new FontName { Val = FONT_NAME },
            Color = new Color { Rgb = FONT_COLOR },
            Bold = new Bold { Val = true }
        };

        var fonts = new Fonts();
        fonts.Append(defaultFont, boldFont);

        return fonts;
    }

    private static Fills GetFills()
    {
        var defaultFill = new Fill
        {
            PatternFill = new PatternFill { PatternType = PatternValues.None }
        };

        var grayFill = new Fill
        {
            PatternFill = new PatternFill { PatternType = PatternValues.Gray125 }
        };

        var yellowFill = new Fill
        {
            PatternFill = new PatternFill
            {
                ForegroundColor = new ForegroundColor { Rgb = "FFFFE1" },
                PatternType = PatternValues.Solid
            }
        };

        var fills = new Fills();
        fills.Append(defaultFill, grayFill, yellowFill);

        return fills;
    }

    private static Borders GetBorders()
    {
        var border = new Border
        {
            TopBorder = new TopBorder(),
            LeftBorder = new LeftBorder(),
            RightBorder = new RightBorder(),
            BottomBorder = new BottomBorder(),
            DiagonalBorder = new DiagonalBorder(),
        };

        var border2 = new Border
        {
            TopBorder = new TopBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "D4D4D4" } },
            LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "D4D4D4" } },
            RightBorder = new RightBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "D4D4D4" } },
            BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin, Color = new Color { Rgb = "D4D4D4" } },
            DiagonalBorder = new DiagonalBorder(),
        };

        var borders = new Borders();
        borders.Append(border, border2);

        return borders;
    }

    private static (CellFormats CellFormats, Dictionary<string, uint> StyleIndexDict)GetCellFormats(Dictionary<string, uint> formatsDict)
    {
        var cellFormats = new CellFormats();

        var defaultFormat = new CellFormat()
        {
            FontId = 0,
            FillId = 0,
            BorderId = 0,
        };

        var wrapTextFormat = new CellFormat()
        {
            FontId = 0,
            FillId = 0,
            BorderId = 0,
            Alignment = new Alignment { WrapText = true }
        };

        var headerFormat = new CellFormat()
        {
            FontId = 1,
            FillId = 0,
            BorderId = 0,
            Alignment = new Alignment { WrapText = true }
        };

        var nullFormat = new CellFormat()
        {
            FontId = 0,
            FillId = 2,
            BorderId = 1,
            ApplyFill = true,
            Alignment = new Alignment { WrapText = true }
        };

        var cellFormatArray = new CellFormat[] { defaultFormat, wrapTextFormat, headerFormat, nullFormat };
        cellFormats.Append(cellFormatArray);

        var styleIndexDict = new Dictionary<string, uint>();
        var styleIndex = (uint)cellFormatArray.Length;
        foreach (var item in formatsDict)
        {
            if (styleIndexDict.TryGetValue(item.Key, out _))
            {
                continue;
            }

            var customFormat = new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                NumberFormatId = item.Value,
                ApplyNumberFormat = true
            };

            cellFormats.AppendChild(customFormat);

            styleIndexDict.Add(item.Key, styleIndex++);
        }

        return (cellFormats, styleIndexDict);
    }

    private static (NumberingFormats NumeringFormats, Dictionary<string, uint> FormatsDict) GetNumberingFormats(ExcelSheetData[] excelSheetDatas)
    {
        var formats = new NumberingFormats();
        var formatsDict = new Dictionary<string, uint>();
        var formatId = 164u;

        foreach (var (key, value) in GetDefaultFormats())
        {
            if (formatsDict.TryGetValue(key, out _))
            {
                continue;
            }

            var numberingFormat = new NumberingFormat
            {
                NumberFormatId = formatId,
                FormatCode = value
            };

            formats.AppendChild(numberingFormat);
            formatsDict.Add(key, formatId++);
        }

        foreach (var item in excelSheetDatas)
        {
            foreach (var formatCode in item.ColumnProperties.Where(x => !string.IsNullOrWhiteSpace(x.Format)).Select(x => x.Format))
            {
                if (formatsDict.TryGetValue(formatCode, out _))
                {
                    continue;
                }

                var numberingFormat = new NumberingFormat
                {
                    NumberFormatId = formatId,
                    FormatCode = formatCode
                };

                formats.AppendChild(numberingFormat);
                formatsDict.Add(formatCode, formatId++);
            }
        }
        
        return (formats, formatsDict);
    }

    private static Dictionary<string, string> GetDefaultFormats()
    {
        var formats = new Dictionary<string, string>
        {
            { Constants.Types.Float, "#,##0.00" },
            { Constants.Types.Double, "#,##0.00" },
            { Constants.Types.Decimal, "#,##0.00" },
            { Constants.Types.DateTime, "dd.mm.yyyy hh:mm.ss" },
            { Constants.Types.DateTimeOffset, "dd.mm.yyyy hh:mm.ss" },
        };

        return formats;
    }

    //https://stackoverflow.com/a/961504
    [GeneratedRegex("(?<![\\uD800-\\uDBFF])[\\uDC00-\\uDFFF]|[\\uD800-\\uDBFF](?![\\uDC00-\\uDFFF])|[\\x00-\\x08\\x0B\\x0C\\x0E-\\x1F\\x7F-\\x9F\\uFEFF\\uFFFE\\uFFFF]", RegexOptions.Compiled)]
    private static partial Regex InvalidXmlCharsRegex();

    private static string RemoveInvalidXMLChars(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return InvalidXmlCharsRegex().Replace(text.Trim(), "");
    }
}
