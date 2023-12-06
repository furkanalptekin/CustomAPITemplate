using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Extensions;

namespace CustomAPITemplate.DB.Repositories;

//TODO: add pagination
public class Repository<TKey, TEntity> : IRepository<TKey, TEntity>
    where TEntity : EntityBase<TKey>
{
    protected readonly AppDbContext _context;
    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public virtual async Task<Response<TEntity>> CreateAsync(TEntity entity, CancellationToken token)
    {
        return await _context.AddEntityAsync<TKey, TEntity>(entity, token).ConfigureAwait(false);
    }

    public virtual async Task<Response<int>> DeleteAsync(TKey id, CancellationToken token)
    {
        return await _context.RemoveEntityAsync<TKey, TEntity>(id, token).ConfigureAwait(false);
    }

    public virtual async Task<Response<IEnumerable<TEntity>>> GetAsync(CancellationToken token)
    {
        return await _context.WhereAsync<TKey, TEntity>((entity) => entity.IsActive, token).ConfigureAwait(false);
    }

    public virtual async Task<Response<TEntity>> GetAsync(TKey id, CancellationToken token)
    {
        return await _context.FindEntityAsync<TKey, TEntity>(id, token).ConfigureAwait(false);
    }

    public virtual async Task<Response<int>> UpdateAsync(TKey id, TEntity entity, string[] propertiesToIgnore, CancellationToken token)
    {
        return await _context.UpdateEntityAsync(id, entity, token, propertiesToIgnore ?? Core.Constants.PropertiesToIgnore.DEFAULT).ConfigureAwait(false);
    }

    public virtual async Task<Response<IEnumerable<TEntity>>> WhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token, params string[] includes)
    {
        return await _context.WhereAsync<TKey, TEntity>(predicate, token, includes).ConfigureAwait(false);
    }
}