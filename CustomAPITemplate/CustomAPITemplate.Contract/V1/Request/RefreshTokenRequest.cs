namespace CustomAPITemplate.Contract.V1;

public class RefreshTokenRequest : IRequestBase
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}