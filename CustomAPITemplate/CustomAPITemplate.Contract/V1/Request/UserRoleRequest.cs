namespace CustomAPITemplate.Contract.V1;
public class UserRoleRequest : IRequestBase
{
    public string UserId { get; set; }
    public string Role { get; set; }
}