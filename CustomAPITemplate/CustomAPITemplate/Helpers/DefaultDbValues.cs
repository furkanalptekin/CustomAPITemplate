using CustomAPITemplate.Core.Constants;
using CustomAPITemplate.DB.Models;
using Microsoft.AspNetCore.Identity;

namespace CustomAPITemplate.Helpers;

public class DefaultDbValues
{
    public static async Task CreateDefaultUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        await CreateRole(roleManager, Roles.ADMIN);
        await CreateRole(roleManager, Roles.MODERATOR);
        await CreateRole(roleManager, Roles.USER);

        await CreateUser(userManager, Roles.ADMIN);
        await CreateUser(userManager, Roles.MODERATOR);
        await CreateUser(userManager, Roles.USER);
    }

    private static async Task CreateRole(RoleManager<AppRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole(role));
        }
    }

    private static async Task CreateUser(UserManager<AppUser> userManager, string role)
    {
        var email = $"{role.ToLower()}@test.com";
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new AppUser
            {
                Email = email,
                UserName = role,
                FirstName = role,
                LastName = "Test",
            };
            await userManager.CreateAsync(user, "Test123+");
            await userManager.AddToRoleAsync(user, role);
        }
    }
}