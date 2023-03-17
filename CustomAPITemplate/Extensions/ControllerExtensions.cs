using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core.Extensions;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Extensions;

public static class ControllerExtensions
{
    public static async Task<IActionResult> GetExtension<TKey, TEntity, TEntityResponse>(this ControllerBase controller, IRepository<TKey, TEntity> repository, IMapper mapper, CancellationToken token, params object[] includes)
        where TEntity : IEntityBase<TKey>
        where TEntityResponse : IAuditResponseBase<TKey>
    {
        var response = await repository.GetAsync(token).ConfigureAwait(false);
        if (!response.Success)
        {
            return controller.NoContent();
        }

        var dtoResponse = mapper.MapResponse<IEnumerable<TEntity>, IEnumerable<TEntityResponse>>(response);
        return controller.Ok(dtoResponse);
    }

    public static async Task<IActionResult> GetExtension<TKey, TEntity, TEntityResponse>(this ControllerBase controller, TKey id, IRepository<TKey, TEntity> repository, IMapper mapper, CancellationToken token, params object[] includes)
        where TEntity : IEntityBase<TKey>
        where TEntityResponse : IAuditResponseBase<TKey>
    {
        var response = await repository.GetAsync(id, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return controller.NoContent();
        }

        var dtoResponse = mapper.MapResponse<TEntity, TEntityResponse>(response);
        return controller.Ok(dtoResponse);
    }

    public static async Task<IActionResult> PostExtension<TKey, TEntity, TEntityRequest, TEntityResponse>(this ControllerBase controller, IRepository<TKey, TEntity> repository, TEntityRequest request, IMapper mapper, CancellationToken token)
        where TEntity : IEntityBase<TKey>
        where TEntityRequest : IRequestBase
        where TEntityResponse : IAuditResponseBase<TKey>
    {
        var entity = mapper.Map<TEntity>(request);

        var dataResponse = entity.InjectData<TKey, TEntity>(controller);
        if (!dataResponse.Success)
        {
            return controller.BadRequest(dataResponse);
        }

        var response = await repository.CreateAsync(entity, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return controller.BadRequest(response);
        }
        //TODO: Add includes to response entity
        return controller.CreatedAtAction("Get", new { id = response.Value.Id }, mapper.MapResponse<TEntity, TEntityResponse>(response));
    }

    public static async Task<IActionResult> DeleteExtension<TKey, TEntity>(this ControllerBase controller, IRepository<TKey, TEntity> repository, TKey id, CancellationToken token)
        where TEntity : IEntityBase<TKey>
    {
        //TODO: update userId, updateTime and updateIp
        var response = await repository.DeleteAsync(id, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return controller.BadRequest(response);
        }

        return controller.NoContent();
    }

    public static async Task<IActionResult> PutExtension<TKey, TEntity, TEntityRequest>(this ControllerBase controller, IRepository<TKey, TEntity> repository, TKey id, TEntityRequest request, IMapper mapper, string[] propertiesToIgnore, CancellationToken token)
        where TEntity : IEntityBase<TKey>
        where TEntityRequest : IRequestBase
    {
        var entity = mapper.Map<TEntity>(request);

        var dataResponse = entity.InjectData<TKey, TEntity>(controller, false);
        if (!dataResponse.Success)
        {
            return controller.BadRequest(dataResponse);
        }

        var response = await repository.UpdateAsync(id, entity, propertiesToIgnore, token);

        if (!response.Success)
        {
            return controller.BadRequest(response);
        }

        return controller.NoContent();
    }
}