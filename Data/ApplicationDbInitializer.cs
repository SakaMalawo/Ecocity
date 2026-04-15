using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using EcoCity.Models;
using EcoCity.Services;

namespace EcoCity.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, 
                                              RoleManager<IdentityRole> roleManager,
                                              IServiceProvider serviceProvider)
        {
            string[] roleNames = { "Admin", "Moderator", "User" };
            
            // Création des rôles s'ils n'existent pas
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Création de l'utilisateur administrateur par défaut
            string adminEmail = "admin@ecocity.com";
            string adminPassword = "Admin@123"; // À changer en production

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "System",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Address = "",
                    City = "",
                    PostalCode = "",
                    Bio = "",
                    Location = "",
                    ProfilePicture = "",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "Moderator");
                    
                    // Créer l'entrée dans la table Admin
                    var adminService = serviceProvider.GetRequiredService<IAdminService>();
                    await adminService.CreateAdminAsync(adminUser.Id, "Admin", "System");
                }
            }

            // Création d'un modérateur de test
            string moderatorEmail = "moderator@ecocity.com";
            string moderatorPassword = "Moderator@123"; // À changer en production

            if (await userManager.FindByEmailAsync(moderatorEmail) == null)
            {
                var moderatorUser = new ApplicationUser
                {
                    UserName = moderatorEmail,
                    Email = moderatorEmail,
                    FirstName = "Moderator",
                    LastName = "Test",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Address = "",
                    City = "",
                    PostalCode = "",
                    Bio = "",
                    Location = "",
                    ProfilePicture = "",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createModerator = await userManager.CreateAsync(moderatorUser, moderatorPassword);
                if (createModerator.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderatorUser, "Moderator");
                }
            }
        }
    }
}
