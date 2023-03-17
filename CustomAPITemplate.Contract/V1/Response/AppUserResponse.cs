using CustomAPITemplate.Contract.V1.Response;

namespace CustomAPITemplate.Contract.V1;

public class AppUserResponse : IResponseBase
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}