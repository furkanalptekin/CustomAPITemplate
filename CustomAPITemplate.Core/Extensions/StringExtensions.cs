namespace CustomAPITemplate.Core.Extensions;

public static class StringExtensions
{
    public static string ToLowerTR(this string str)
        => str.ToLower(Constants.Cultures.TR);

    public static string ToLowerEN(this string str)
        => str.ToLower(Constants.Cultures.EN);

    public static bool ContainsLoweredTR(this string str, string compare)
        => str.ToLowerTR().Contains(compare?.ToLowerTR());

    public static bool ContainsLoweredEN(this string str, string compare)
        => str.ToLowerEN().Contains(compare?.ToLowerEN());
}