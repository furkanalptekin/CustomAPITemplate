using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core.Extensions;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Extensions;

public static class ControllerExtensions
{
    public static async Task<IActionResult> GetExtension<TEntity, TEntityResponse>(this ControllerBase controller, IRepository<TEntity> repository, IMapper mapper, CancellationToken token, params object[] includes)
        where TEntity : IEntityBase
        where TEntityResponse : IResponseBase
    {
        var response = await repository.GetAsync(token).ConfigureAwait(false);
        var dtoResponse = mapper.MapResponse<IEnumerable<TEntity>, IEnumerable<TEntityResponse>>(response);
        //TODO: ??
        return controller.Ok(dtoResponse);
    }

    public static async Task<IActionResult> GetExtension<TEntity, TEntityResponse>(this ControllerBase controller, Guid id, IRepository<TEntity> repository, IMapper mapper, CancellationToken token, params object[] includes)
        where TEntity : IEntityBase
        where TEntityResponse : IResponseBase
    {
        var response = await repository.GetAsync(id, token).ConfigureAwait(false);
        var dtoResponse = mapper.MapResponse<TEntity, TEntityResponse>(response);

        if (!response.Success)
        {
            return controller.NoContent();
        }

        return controller.Ok(dtoResponse);
    }

    public static async Task<IActionResult> PostExtension<TEntity, TEntityRequest, TEntityResponse>(this ControllerBase controller, IRepository<TEntity> repository, TEntityRequest request, IMapper mapper, CancellationToken token)
        where TEntity : IEntityBase
        where TEntityRequest : IRequestBase
        where TEntityResponse : IResponseBase
    {
        //TODO: add fluent validation
        if (controller.ModelState.IsValid)
        {
            var entity = mapper.Map<TEntity>(request);
            
            var dataResponse = entity.InjectData(controller);
            if (!dataResponse.Success)
            {
                return controller.BadRequest(dataResponse);
            }

            var response = await repository.CreateAsync(entity, token).ConfigureAwait(false);
            if (!response.Success)
            {
                return controller.BadRequest(response);
            }

            return controller.CreatedAtAction("Get", new { id = response.Value.Id }, mapper.MapResponse<TEntity, TEntityResponse>(response));
        }

        return controller.BadRequest(controller.ModelState);
    }

    public static async Task<IActionResult> DeleteExtension<TEntity>(this ControllerBase controller, IRepository<TEntity> repository, Guid id, CancellationToken token)
        where TEntity : IEntityBase
    {
        //TODO: update userId, updateTime and updateIp
        var response = await repository.DeleteAsync(id, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return controller.BadRequest(response);
        }

        return controller.NoContent();
    }

    public static async Task<IActionResult> PutExtension<TEntity, TEntityRequest>(this ControllerBase controller, IRepository<TEntity> repository, Guid id, TEntityRequest request, IMapper mapper, string[] propertiesToIgnore, CancellationToken token)
        where TEntity : IEntityBase
        where TEntityRequest : IRequestBase
    {
        var entity = mapper.Map<TEntity>(request);

        var dataResponse = entity.InjectData(controller, false);
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