using CustomAPITemplate.DB.Entity;

namespace CustomAPITemplate.DB.Models;

public class Example : AuditEntityBase<int>
{
    public string Test1 { get; set; }
    public string Test2 { get; set; }
}