using AutoMapper;
using CustomAPITemplate.Attributes;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Repositories;
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
        where TEntityResponse : IAuditResponseBase<TKey>
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
#if (Cache != "NoCache")
    [Cache]
#endif
    public virtual async Task<IActionResult> Get(CancellationToken token)
    {
        var response = await _repository.GetAsync(token).ConfigureAwait(false);
        if (!response.Success)
        {
            return NoContent();
        }

        var dtoResponse = _mapper.MapResponse<IEnumerable<TEntity>, IEnumerable<TEntityResponse>>(response);
        return Ok(dtoResponse);
    }

    [HttpGet("{id}")]
#if (Cache != "NoCache")
    [Cache]
#endif
    public virtual async Task<IActionResult> Get(TKey id, CancellationToken token)
    {
        var response = await _repository.GetAsync(id, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return NoContent();
        }

        var dtoResponse = _mapper.MapResponse<TEntity, TEntityResponse>(response);
        return Ok(dtoResponse);
    }

    [HttpPost]
#if (Cache != "NoCache")
    [ClearCache(typeof(CreatedAtActionResult))]
#endif
    [Transaction]
    public virtual async Task<IActionResult> Post(TEntityRequest entity, CancellationToken token)
    {
        var request = _mapper.Map<TEntity>(entity);
        var response = await _repository.CreateAsync(request, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        var id = response.Value.Id;
        var dbEntityResponse = await _repository.GetAsync(response.Value.Id, token).ConfigureAwait(false);

        return CreatedAtAction("Get", new { id }, _mapper.MapResponse<TEntity, TEntityResponse>(dbEntityResponse.Success ? dbEntityResponse : response));
    }

    [HttpDelete("{id}")]
#if (Cache != "NoCache")
    [ClearCache(typeof(NoContentResult))]
#endif
    [Transaction]
    public virtual async Task<IActionResult> Delete(TKey id, CancellationToken token)
    {
        var response = await _repository.DeleteAsync(id, token).ConfigureAwait(false);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return NoContent();
    }

    [HttpPut("{id}")]
#if (Cache != "NoCache")
    [ClearCache(typeof(NoContentResult))]
#endif
    [Transaction]
    public virtual async Task<IActionResult> Put(TKey id, TEntityRequest entity, CancellationToken token)
    {
        var request = _mapper.Map<TEntity>(entity);
        var response = await _repository.UpdateAsync(id, request, null, token);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return NoContent();
    }

    [HttpPatch]
    public virtual async Task<IActionResult> Patch()
    {
        //TODO: Patch
        await Task.Delay(1);
        return Ok();
    }
}