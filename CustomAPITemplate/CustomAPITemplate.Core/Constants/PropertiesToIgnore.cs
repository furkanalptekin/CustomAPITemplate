namespace CustomAPITemplate.Core.Constants;

public static class PropertiesToIgnore
{
    public static readonly string[] DEFAULT = ["Id", "CreationTime", "CreatorUserId", "HostIP", "IsActive"];
    public static readonly string[] DEFAULT_WITH_FILE = ["Id", "CreationTime", "CreatorUserId", "HostIP", "IsActive", "FilePath"];
}