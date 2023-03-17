using AutoMapper;
using CustomAPITemplate.Attributes;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Repositories.Interfaces;
using CustomAPITemplate.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Helpers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BasicApiController<TKey, TEntity, TEntityRequest, TEntityResponse, TRepository>
    : ControllerBase, IApiBase<TKey, TEntityRequest>
        where TEntity : IEntityBase<TKey>
        where TEntityRequest : IRequestBase
        where TEntityResponse: IAuditResponseBase<TKey>
        where TRepository : IRepository<TKey, TEntity>
{

    protected readonly TRepository _repository;
    protected readonly IMapper _mapper;

    public BasicApiController(TRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    [Cache]
    public virtual async Task<IActionResult> Get(CancellationToken token)
    {
        return await this.GetExtension<TKey, TEntity, TEntityResponse>(_repository, _mapper, token).ConfigureAwait(false);
    }

    [HttpGet("{id}")]
    [Cache]
    public virtual async Task<IActionResult> Get(TKey id, CancellationToken token)
    {
        return await this.GetExtension<TKey, TEntity, TEntityResponse>(id, _repository, _mapper, token).ConfigureAwait(false);
    }

    [HttpPost]
    [ClearCache(typeof(CreatedAtActionResult))]
    [Transaction]
    public virtual async Task<IActionResult> Post(TEntityRequest entity, CancellationToken token)
    {
        return await this.PostExtension<TKey, TEntity, TEntityRequest, TEntityResponse>(_repository, entity, _mapper, token).ConfigureAwait(false);
    }

    [HttpDelete("{id}")]
    [ClearCache(typeof(NoContentResult))]
    [Transaction]
    public virtual async Task<IActionResult> Delete(TKey id, CancellationToken token)
    {
        return await this.DeleteExtension(_repository, id, token).ConfigureAwait(false);
    }

    [HttpPut("{id}")]
    [ClearCache(typeof(NoContentResult))]
    [Transaction]
    public virtual async Task<IActionResult> Put(TKey id, TEntityRequest entity, CancellationToken token)
    {
        return await this.PutExtension(_repository, id, entity, _mapper, null, token).ConfigureAwait(false);
    }

    [HttpPatch]
    public virtual async Task<IActionResult> Patch()
    {
        //TODO: Patch
        await Task.Delay(1);
        return Ok();
    }
}