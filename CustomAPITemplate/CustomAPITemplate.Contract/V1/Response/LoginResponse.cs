using CustomAPITemplate.Contract.V1.Response;

namespace CustomAPITemplate.Contract.V1;

public class LoginResponse : IResponseBase
{
    public string Token { get; set; }
    public Guid RefreshToken { get; set; }
}