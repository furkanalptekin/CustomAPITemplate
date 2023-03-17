using AutoMapper;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories.Interfaces;

namespace CustomAPITemplate.Controllers.V1;

public class ExampleController : BasicV1Controller<int, Example, ExampleRequest, ExampleResponse, IExampleRepository>
{
    public ExampleController(IExampleRepository repository, IMapper mapper) 
        : base(repository, mapper)
    {

    }
}