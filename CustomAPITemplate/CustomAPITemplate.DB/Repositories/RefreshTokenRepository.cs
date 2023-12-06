using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Repositories;

public class RefreshTokenRepository : Repository<Guid, RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Response<int>> DeleteAsync(RefreshToken entity, CancellationToken token)
    {
        var response = new Response<int>();

        entity.IsActive = false;

        var changes = await _context.SaveChangesAsync(token).ConfigureAwait(false);
        if (changes <= 0)
        {
            response.Value = changes;
            return response.AddError("Error occurred during savechanges!");
        }

        response.Value = changes;
        return response.AddInfo("Successfully deleted");
    }

    public async Task<Response<int>> InvalidateTokensByUserId(Guid id, CancellationToken token)
    {
        var response = new Response<int>();
        var changes = await _context.RefreshToken.Where(x => x.UserId == id)
            .ExecuteUpdateAsync(prop => prop.SetProperty(x => x.Invalidated, true), token)
            .ConfigureAwait(false);

        if (changes == 0)
        {
            return response.AddError("Did not find a token to invalidate");
        }

        response.Value = changes;
        return response.AddInfo("Successfully invalidated");
    }
}