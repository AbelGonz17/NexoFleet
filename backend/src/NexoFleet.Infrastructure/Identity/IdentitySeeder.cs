using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexoFleet.Application.Authorization;

namespace NexoFleet.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(BootstrapSuperAdminOptions.SectionName)
            .Get<BootstrapSuperAdminOptions>();

        if (options is not { Enabled: true })
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "BootstrapSuperAdmin requires an email and password when enabled.");
        }

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in UserRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                EnsureSucceeded(result, $"creating role {role}");
            }
        }

        var user = await userManager.FindByEmailAsync(options.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = options.Email.Trim(),
                Email = options.Email.Trim(),
                EmailConfirmed = true,
                FirstName = options.FirstName.Trim(),
                LastName = options.LastName.Trim(),
                IsActive = true
            };

            EnsureSucceeded(
                await userManager.CreateAsync(user, options.Password),
                "creating the bootstrap SuperAdmin");
        }

        if (!await userManager.IsInRoleAsync(user, UserRoles.SuperAdmin))
        {
            EnsureSucceeded(
                await userManager.AddToRoleAsync(user, UserRoles.SuperAdmin),
                "assigning the SuperAdmin role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Identity failed while {operation}: {errors}");
    }
}
