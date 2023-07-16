using Microsoft.AspNetCore.Identity;

namespace CustomAPITemplate.DB.Models;

public class AppRole : IdentityRole<Guid>
{
    public AppRole(): base()
    {

    }

    public AppRole(string roleName): base(roleName)
    {

    }
}