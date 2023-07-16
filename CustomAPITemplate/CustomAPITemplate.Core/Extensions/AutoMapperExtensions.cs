using AutoMapper;

namespace CustomAPITemplate.Core.Extensions;

public static class AutoMapperExtensions
{
    public static Response<TDestination> MapResponse<T, TDestination>(this IMapper mapper, Response<T> response)
    {
        var tempResponse = new Response<TDestination>();

        if (response.Value != null)
        {
            var mappedValue = mapper.Map<TDestination>(response.Value);
            tempResponse.Value = mappedValue;
        }

        tempResponse.Results.AddRange(response.Results);
        return tempResponse;
    }
}