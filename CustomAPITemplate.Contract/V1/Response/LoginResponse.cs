namespace CustomAPITemplate.Contract.V1;

public class LoginResponse
{
    public string Token { get; set; }
    public Guid RefreshToken { get; set; }
}