using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CustomAPITemplate.Core;

public static class JsonExtensions
{
    private static readonly DefaultContractResolver _resolver = new()
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    };

    private static readonly JsonSerializerSettings _settings = new()
    {
        ContractResolver = _resolver
    };

    public static string ToJson<T>(this Response<T> response)
    {
        return JsonConvert.SerializeObject(response, _settings);
    }

    public static string ToJson(this object response)
    {
        return JsonConvert.SerializeObject(response, _settings);
    }
}