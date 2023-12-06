namespace CustomAPITemplate.Contract.V1;

public class ExampleRequest : IRequestBase, ISanitizable
{
    public string Test1 { get; set; }
    public string Test2 { get; set; }

    public string[] PropertiesToSanitize => [nameof(Test1), nameof(Test2)];
}