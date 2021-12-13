namespace CustomAPITemplate.Core.Constants;

public static class Roles
{
    public const string ADMIN = "Admin";
    public const string MODERATOR = "Moderator";
    public const string USER = "User";
}

public static class MinAllowedRole
{
    public const string ADMIN = "Admin";
    public const string MODERATOR = "Admin,Moderator";
    public const string USER = "Admin,Moderator,User";
}