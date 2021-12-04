using AutoMapper;
using CustomAPITemplate.DB.Entity;
using CustomAPITemplate.DB.Models;

namespace CustomAPITemplate.Contract.V1;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, AppUserResponse>()
            .ForMember(dest => dest.ImagePath,
                opt => opt.MapFrom(src => src.ImagePath ?? @"Files/defaultAvatar.png"));

        CreateMap<AppUserResponse, AppUser>();

        CreateMap<EntityBase, ResponseBase>()
            .IncludeAllDerived();

        //CreateMap<FileEntityBase, FileEntityBaseDto>()
        //    .IncludeAllDerived();

        CreateMap<Example, ExampleResponse>().ReverseMap();
        CreateMap<Example, ExampleRequest>().ReverseMap();
    }
}