using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Models;

namespace CustomAPITemplate.DB.Repositories;

public interface IRefreshTokenRepository : IRepository<Guid, RefreshToken>
{
    Task<Response<int>> DeleteAsync(RefreshToken entity, CancellationToken token);

    Task<Response<int>> InvalidateTokensByUserId(Guid id, CancellationToken token);
}