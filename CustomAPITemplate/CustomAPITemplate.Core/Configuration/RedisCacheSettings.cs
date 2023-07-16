namespace CustomAPITemplate.Core.Configuration;

public class RedisCacheSettings
{
    public bool IsEnabled { get; set; }
    public string ConnectionString { get; set; }
}