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
public class BasicAPIController<TEntity, TEntityRequest, TEntityResponse, TRepository>
    : ControllerBase, IAPIBase<TEntityRequest>
        where TEntity : IEntityBase
        where TEntityRequest : IRequestBase
        where TEntityResponse: IResponseBase
        where TRepository : IRepository<TEntity>
{

    protected readonly TRepository _repository;
    protected readonly IMapper _mapper;

    public BasicAPIController(TRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    [Cache]
    public virtual async Task<IActionResult> Get(CancellationToken token)
    {
        return await this.GetExtension<TEntity, TEntityResponse>(_repository, _mapper, token).ConfigureAwait(false);
    }

    [HttpGet("{id}")]
    [Cache]
    public virtual async Task<IActionResult> Get(Guid id, CancellationToken token)
    {
        return await this.GetExtension<TEntity, TEntityResponse>(id, _repository, _mapper, token).ConfigureAwait(false);
    }

    [HttpPost]
    [ClearCache(typeof(CreatedAtActionResult))]
    public virtual async Task<IActionResult> Post(TEntityRequest entity, CancellationToken token)
    {
        return await this.PostExtension<TEntity, TEntityRequest, TEntityResponse>(_repository, entity, _mapper, token).ConfigureAwait(false);
    }

    [HttpDelete("{id}")]
    [ClearCache(typeof(NoContentResult))]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        return await this.DeleteExtension(_repository, id, token).ConfigureAwait(false);
    }

    [HttpPut("{id}")]
    [ClearCache(typeof(NoContentResult))]
    public virtual async Task<IActionResult> Put(Guid id, TEntityRequest entity, CancellationToken token)
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