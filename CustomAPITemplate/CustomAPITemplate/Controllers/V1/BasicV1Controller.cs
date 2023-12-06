using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Repositories;
using CustomAPITemplate.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class BasicV1Controller<TKey, TEntity, TEntityRequest, TEntityResponse, TRepository>
    : BasicApiController<TKey, TEntity, TEntityRequest, TEntityResponse, TRepository>
        where TEntity : IEntityBase<TKey>
        where TEntityRequest : IRequestBase
        where TEntityResponse : IAuditResponseBase<TKey>
        where TRepository : IRepository<TKey, TEntity>
{
    public BasicV1Controller(TRepository repository, IMapper mapper)
        : base(repository, mapper)
    {

    }
}