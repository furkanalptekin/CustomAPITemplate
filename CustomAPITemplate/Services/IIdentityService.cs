using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core;

namespace CustomAPITemplate.Services;

public interface IIdentityService
{
    Task<Response> RegisterAsync(RegistrationRequest request);

    Task<Response<LoginResponse>> LoginAsync(LoginRequest request);

    Task<Response<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<Response> BanUserAsync(Guid id);
}