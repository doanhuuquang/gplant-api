using Microsoft.AspNetCore.Identity;
using Gplant.Domain.Constants;
using Gplant.Domain.Entities;

namespace Gplant.Infrastructure.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAdminAsync(UserManager<User> userManager)
        {
            var email = "admin@gplant.com";
            var password = "AdminGplant@2025";

            var admin = await userManager.FindByEmailAsync(email);
            if (admin != null) return;

            admin = new User
            {
                FirstName       = "System",
                LastName        = "Administrator",
                UserName        = email,
                Email           = email,
                EmailConfirmed  = true
            };

            await userManager.CreateAsync(admin, password);
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
