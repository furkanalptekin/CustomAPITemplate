using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;

namespace CustomAPITemplate.DB.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : IEntityBase
{
    public Task<Response<TEntity>> CreateAsync(TEntity entity, CancellationToken token);
    public Task<Response<int>> DeleteAsync(Guid id, CancellationToken token);
    public Task<Response<int>> UpdateAsync(Guid id, TEntity entity, string[] propertiesToIgnore, CancellationToken token);
    public Task<Response<IEnumerable<TEntity>>> GetAsync(CancellationToken token);
    public Task<Response<TEntity>> GetAsync(Guid id, CancellationToken token);
    public Task<Response<IEnumerable<TEntity>>> WhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token, params object[] includes);
}