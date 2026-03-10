using Gplant.Domain.Constants;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Gplant.Infrastructure.Seeds
{
    public class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<Role> roleManager)
        {
            string[] roles =
            [
                Roles.Customer,
                Roles.Manager,
                Roles.Admin,
            ];

            foreach (var role in roles) 
                if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new Role { Name = role });
        }
    }
}
