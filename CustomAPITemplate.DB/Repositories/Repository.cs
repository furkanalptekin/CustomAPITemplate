using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Extensions;
using CustomAPITemplate.DB.Repositories.Interfaces;

namespace CustomAPITemplate.DB.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly AppDbContext _context;
    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Response<TEntity>> CreateAsync(TEntity entity, CancellationToken token)
    {
        return await _context.AddEntityAsync(entity, token).ConfigureAwait(false);
    }

    public async Task<Response<int>> DeleteAsync(Guid id, CancellationToken token)
    {
        return await _context.RemoveEntityAsync<TEntity>(id, token).ConfigureAwait(false);
    }

    public async Task<Response<IEnumerable<TEntity>>> GetAsync(CancellationToken token)
    {
        return await _context.WhereAsync<TEntity>((entity) => entity.IsActive, token).ConfigureAwait(false);
    }

    public async Task<Response<TEntity>> GetAsync(Guid id, CancellationToken token)
    {
        return await _context.FindEntityAsync<TEntity>(id, token).ConfigureAwait(false);
    }

    public async Task<Response<int>> UpdateAsync(Guid id, TEntity entity, string[] propertiesToIgnore, CancellationToken token)
    {
        return await _context.UpdateEntityAsync(id, entity, token, propertiesToIgnore ?? Core.Constants.PropertiesToIgnore.DEFAULT).ConfigureAwait(false);
    }

    public async Task<Response<IEnumerable<TEntity>>> WhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token, params object[] includes)
    {
        return await _context.WhereAsync<TEntity>(predicate, token, includes).ConfigureAwait(false);
    }
}