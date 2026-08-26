using NexoFleet.Application.Authentication;

namespace NexoFleet.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<LoginResult> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SignOutAsync();
}
