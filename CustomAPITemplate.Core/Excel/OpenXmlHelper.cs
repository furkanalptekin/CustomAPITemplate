using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CustomAPITemplate.Core.Excel;

public static partial class OpenXmlHelper
{
    private const string FONT_NAME = "Calibri";
    private const int FONT_SIZE = 11;
    private const string FONT_COLOR = "000000";

    public static Cell GetCell(Type type, object value)
    {
        var cellValue = GetCellValue(type, value, out var isNull, out var hasNewLine);

        return new Cell
        {
            DataType = GetDataType(type),
            CellValue = cellValue,
            StyleIndex = GetCellStyleIndex(isNull, hasNewLine),
        };
    }

    //TODO: add more types
    private static CellValues GetDataType(Type type)
    {
        return CellValues.String;
    }

    private static uint GetCellStyleIndex(bool isNull, bool hasNewLine)
    {
        if (isNull)
        {
            return 3U; // Cell Format 3 -> Yellow Fill
        }

        if (hasNewLine)
        {
            return 1U; // TextWrap: true
        }

        return 0U; // Default
    }

    private static CellValue GetCellValue(Type type, object data, out bool isNull, out bool hasNewLine)
    {
        isNull = false;
        hasNewLine = false;

        if (data == null)
        {
            isNull = true;
            return new CellValue(null);
        }

        //TODO: add more types and culture
        var value = type.ToString() switch
        {
            "System.DateTime" => DateTime.Now.ToString(Constants.Cultures.TR),
            _ => data.ToString(),
        };

        value = RemoveInvalidXMLChars(value);
        hasNewLine = value.Contains('\n');
        return new CellValue(value);
    }

    public static Stylesheet CreateStylesheet()
    {
        return new Stylesheet
        {
            Fonts = GetFonts(),
            Fills = GetFills(),
            Borders = GetBorders(),
            CellFormats = GetCellFormats(),
        };
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

    private static CellFormats GetCellFormats()
    {
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

        var cellFormats = new CellFormats();
        cellFormats.Append(defaultFormat, wrapTextFormat, headerFormat, nullFormat);

        return cellFormats;
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
