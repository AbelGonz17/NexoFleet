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

    Task SignOutAsync();
}
