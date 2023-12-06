using Ganss.Xss;

namespace CustomAPITemplate.Core;

public static class StringExtensions
{
    private static readonly HtmlSanitizer _sanitizer = new();

    public static string ToLowerTR(this string str)
        => str.ToLower(Constants.Cultures.TR);

    public static string ToLowerEN(this string str)
        => str.ToLower(Constants.Cultures.EN);

    public static bool ContainsLoweredTR(this string str, string compare)
        => str.ToLowerTR().Contains(compare?.ToLowerTR());

    public static bool ContainsLoweredEN(this string str, string compare)
        => str.ToLowerEN().Contains(compare?.ToLowerEN());

    public static string Sanitize(this string str)
        => string.IsNullOrWhiteSpace(str) ? str : _sanitizer.Sanitize(str);
}