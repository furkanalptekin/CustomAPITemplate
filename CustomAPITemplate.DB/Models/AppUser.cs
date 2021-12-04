using Microsoft.AspNetCore.Identity;

namespace CustomAPITemplate.DB.Models;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string ImagePath { get; set; }
    public bool IsBanned { get; set; }
}