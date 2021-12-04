using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Repositories.Interfaces;
using CustomAPITemplate.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class BasicV1Controller<TEntity, TEntityRequest, TEntityResponse, TRepository>
    : BasicAPIController<TEntity, TEntityRequest, TEntityResponse, TRepository>
        where TEntity : IEntityBase
        where TEntityRequest : IRequestBase
        where TEntityResponse : IResponseBase
        where TRepository : IRepository<TEntity>
{
    public BasicV1Controller(TRepository repository, IMapper mapper)
        : base(repository, mapper)
    {

    }
}