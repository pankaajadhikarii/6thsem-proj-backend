using Bizkit_backend.Configuration;
using Bizkit_backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bizkit_backend.Services.Admin;

public sealed class AdminSetupService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<AdminSettings> adminSettings) : IAdminSetupService
{
    private const string CustomerRole = "Customer";
    private const string AdminRole = "Admin";

    public async Task SetupAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync(CustomerRole);
        await EnsureRoleAsync(AdminRole);

        await EnsureAdminAsync(adminSettings.Value.Account1);
        await EnsureAdminAsync(adminSettings.Value.Account2);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(
            new IdentityRole(roleName));

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Failed to create role '{roleName}': {errors}");
        }
    }

    private async Task EnsureAdminAsync(AdminAccount settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Email) ||
            string.IsNullOrWhiteSpace(settings.Password) ||
            string.IsNullOrWhiteSpace(settings.FullName))
        {
            throw new InvalidOperationException(
                "Admin account configuration is incomplete.");
        }

        var email = settings.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = settings.FullName.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(
                user,
                settings.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    createResult.Errors.Select(
                        error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to create admin '{email}': {errors}");
            }
        }

        if (!user.IsActive)
        {
            user.IsActive = true;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    updateResult.Errors.Select(
                        error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to activate admin '{email}': {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, AdminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user,
                AdminRole);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(
                        error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to assign Admin role to '{email}': {errors}");
            }
        }
    }
}