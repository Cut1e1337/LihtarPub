using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;
using Lihtar.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ArtPubDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        await db.Database.MigrateAsync();

        // Roles
        if (!await roleManager.RoleExistsAsync(UserRole.Admin.ToString()))
            await roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.Admin.ToString()));

        if (!await roleManager.RoleExistsAsync(UserRole.Client.ToString()))
            await roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.Client.ToString()));

        // Admin user (ALWAYS ensure password and role)
        var adminEmail = "admin@artpub.com";
        var adminPassword = "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "ArtPub Admin",
                Role = UserRole.Admin,
                EmailConfirmed = true,
                IsBlocked = false
            };

            var created = await userManager.CreateAsync(admin, adminPassword);
            if (!created.Succeeded)
                throw new Exception(string.Join("; ", created.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
        }
        else
        {
            // ensure confirmed
            if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                await userManager.UpdateAsync(admin);
            }

            // ensure role
            if (!await userManager.IsInRoleAsync(admin, UserRole.Admin.ToString()))
                await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());

            // force password
            var token = await userManager.GeneratePasswordResetTokenAsync(admin);
            var reset = await userManager.ResetPasswordAsync(admin, token, adminPassword);
            if (!reset.Succeeded)
                throw new Exception(string.Join("; ", reset.Errors.Select(e => e.Description)));
        }

        if (!db.MenuItemTags.Any())
        {
            db.MenuItemTags.AddRange(
                new MenuItemTag { Id = Guid.NewGuid(), Name = "Vegan" },
                new MenuItemTag { Id = Guid.NewGuid(), Name = "Spicy" },
                new MenuItemTag { Id = Guid.NewGuid(), Name = "Alcohol" },
                new MenuItemTag { Id = Guid.NewGuid(), Name = "Non-alcohol" }
            );

            await db.SaveChangesAsync();
        }


    }
}
