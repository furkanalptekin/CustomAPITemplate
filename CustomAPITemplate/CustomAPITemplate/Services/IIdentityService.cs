using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core;

namespace CustomAPITemplate.Services;

public interface IIdentityService
{
    Task<Response> RegisterAsync(RegistrationRequest request);

    Task<Response<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken token);

    Task<Response<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken token);

    Task<Response> BanUserAsync(Guid id, CancellationToken token);

    Task<Response> InvalidateRefreshTokensByUserId(Guid id, CancellationToken token);

    Task<Response> ChangeUserRole(UserRoleRequest request, CancellationToken token);
}