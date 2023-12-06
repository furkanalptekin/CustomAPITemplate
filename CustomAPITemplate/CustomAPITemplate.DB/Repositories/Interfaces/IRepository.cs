using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;

namespace CustomAPITemplate.DB.Repositories;

public interface IRepository<TKey, TEntity> where TEntity : IEntityBase<TKey>
{
    Task<Response<TEntity>> CreateAsync(TEntity entity, CancellationToken token);
    Task<Response<int>> DeleteAsync(TKey id, CancellationToken token);
    Task<Response<int>> UpdateAsync(TKey id, TEntity entity, string[] propertiesToIgnore, CancellationToken token);
    Task<Response<IEnumerable<TEntity>>> GetAsync(CancellationToken token);
    Task<Response<TEntity>> GetAsync(TKey id, CancellationToken token);
    Task<Response<IEnumerable<TEntity>>> WhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token, params string[] includes);
}