namespace CustomAPITemplate.Core.Constants;

public static class Roles
{
    public const string ADMIN = "Admin";
    public const string USER = "User";
}

public static class MaxAllowedRole
{
    public const string ADMIN = "Admin,User";
    public const string USER = "User";
}

public static class MinAllowedRole
{
    public const string ADMIN = "Admin";
    public const string USER = "Admin,User";
}