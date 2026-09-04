using NexoFleet.Application.Authentication;
using NexoFleet.Domain.Common;

namespace NexoFleet.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<Result<AuthenticatedUser>> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticatedUser>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Guid? companyId,
        string role,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthenticatedUser>> GetUsersByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task SignOutAsync();
}
