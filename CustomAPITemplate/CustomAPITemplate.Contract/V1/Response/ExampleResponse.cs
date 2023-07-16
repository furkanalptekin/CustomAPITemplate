namespace CustomAPITemplate.Contract.V1;

public class ExampleResponse : AuditResponseBase<int>
{
    public string Test1 { get; set; }
    public string Test2 { get; set; }
}