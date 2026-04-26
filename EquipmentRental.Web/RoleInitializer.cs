using EquipmentRental.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace EquipmentRental.Web
{
    /// <summary>
    /// Seeds the Administrator role and a default admin user on application startup.
    /// The admin account (admin@local / P@ssw0rd1) is created only if it does not exist.
    /// Additional administrators cannot be created through the UI per requirements.
    /// </summary>
    public class RoleInitializer
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public RoleInitializer(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            // Ensure both roles exist in the Roles table
            foreach (var role in new[] { "Administrator", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create the default admin account if absent
            const string adminEmail = "admin@local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    EmailConfirmed = true,
                    FirstName      = "System",
                    LastName       = "Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, "P@ssw0rd1");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}
