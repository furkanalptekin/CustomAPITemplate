namespace CustomAPITemplate.Core.Excel;

internal class CellProperties
{
    public Type Type { get; set; }
    public object Value { get; set; }
    public string Format { get; set; }

    public string TypeAsString
    {
        get
        {
            if (Type.IsGenericType)
            {
                if (Type.GenericTypeArguments != null && Type.GenericTypeArguments.Length > 0)
                {
                    return Type.GenericTypeArguments.First().ToString();
                }

                return Constants.Types.String;
            }

            return Type.ToString();
        }
    }
}