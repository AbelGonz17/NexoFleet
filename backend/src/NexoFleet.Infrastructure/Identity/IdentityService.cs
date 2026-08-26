using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Authentication;
using NexoFleet.Domain.Common;

namespace NexoFleet.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<Result<AuthenticatedUser>> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        var user = await userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result<AuthenticatedUser>.Failure(AuthErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result<AuthenticatedUser>.Failure(AuthErrors.Inactive);
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return Result<AuthenticatedUser>.Failure(AuthErrors.LockedOut);
        }

        if (!signInResult.Succeeded)
        {
            return Result<AuthenticatedUser>.Failure(AuthErrors.InvalidCredentials);
        }

        return Result<AuthenticatedUser>.Success(await MapUserAsync(user));
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
