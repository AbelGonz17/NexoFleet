using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Authentication;

namespace NexoFleet.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<LoginResult> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        var user = await userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return LoginResult.Failed(LoginStatus.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return LoginResult.Failed(LoginStatus.Inactive);
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return LoginResult.Failed(LoginStatus.LockedOut);
        }

        if (!signInResult.Succeeded)
        {
            return LoginResult.Failed(LoginStatus.InvalidCredentials);
        }

        return LoginResult.Success(await MapUserAsync(user));
    }

    public async Task<AuthenticatedUser?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        return user is { IsActive: true }
            ? await MapUserAsync(user)
            : null;
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    private async Task<AuthenticatedUser> MapUserAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new AuthenticatedUser(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.CompanyId,
            roles.ToArray());
    }
}
