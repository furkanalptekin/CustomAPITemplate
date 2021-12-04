using System.ComponentModel.DataAnnotations;

namespace CustomAPITemplate.Core.Attributes;
public class IsTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return false;
        }

        if (value.GetType() != typeof(bool))
        {
            return false;
        }

        return (bool)value;
    }
}