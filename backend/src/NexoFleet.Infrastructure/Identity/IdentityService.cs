using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Authentication;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ICompanyRepository companyRepository) : IIdentityService
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

        if (user.CompanyId.HasValue)
        {
            var company = await companyRepository.GetByIdAsync(user.CompanyId.Value, cancellationToken);
            if (company is null || company.Status != CompanyStatus.Active)
            {
                return Result<AuthenticatedUser>.Failure(AuthErrors.CompanyInactive);
            }
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

        return Result<AuthenticatedUser>.Success(await MapUserAsync(user, cancellationToken));
    }

    public async Task<AuthenticatedUser?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        return user is { IsActive: true }
            ? await MapUserAsync(user, cancellationToken)
            : null;
    }

    public async Task<Result<AuthenticatedUser>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Guid? companyId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        var existingUser = await userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return Result<AuthenticatedUser>.Failure(AuthErrors.EmailAlreadyExists);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email.Trim(),
            Email = email.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CompanyId = companyId,
            IsActive = true,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return Result<AuthenticatedUser>.Failure(new ValidationError(errors));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return Result<AuthenticatedUser>.Failure(new ValidationError(errors));
            }
        }

        return Result<AuthenticatedUser>.Success(await MapUserAsync(user, cancellationToken));
    }

    public async Task<IReadOnlyList<AuthenticatedUser>> GetUsersByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Where(u => u.CompanyId == companyId && u.IsActive)
            .ToListAsync(cancellationToken);

        var list = new List<AuthenticatedUser>();
        foreach (var user in users)
        {
            list.Add(await MapUserAsync(user, cancellationToken));
        }

        return list;
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    private async Task<AuthenticatedUser> MapUserAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await userManager.GetRolesAsync(user);
        string? companyName = null;

        if (user.CompanyId.HasValue)
        {
            var company = await companyRepository.GetByIdAsync(user.CompanyId.Value, cancellationToken);
            companyName = company?.Name.Value;
        }

        return new AuthenticatedUser(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.CompanyId,
            companyName,
            roles.ToArray());
    }
}

