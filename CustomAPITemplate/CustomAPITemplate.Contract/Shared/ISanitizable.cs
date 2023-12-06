namespace CustomAPITemplate.Contract;

public interface ISanitizable
{
    public string[] PropertiesToSanitize { get; }
}