using AutoMapper;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Models;

namespace CustomAPITemplate.Contract.V1;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, AppUserResponse>().ReverseMap();

        CreateMap(typeof(EntityBase<>), typeof(AuditResponseBase<>))
            .IncludeAllDerived();

        CreateMap<Example, ExampleResponse>();
        
        CreateMap<Example, ExampleRequest>().ReverseMap();
    }
}