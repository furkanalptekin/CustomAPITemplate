namespace CustomAPITemplate.Contract.V1;

public class LoginRequest : IRequestBase
{
    public string Email { get; set; }
    public string Password { get; set; }
}