using FluentValidation;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Context;

namespace NexoFleet.Application.Authentication;

public sealed class AuthService(
    IIdentityService identityService,
    ICurrentUser currentUser,
    IValidator<LoginRequest> loginValidator)
{
    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        return await identityService.PasswordSignInAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);
    }

    public Task<AuthenticatedUser?> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        return currentUser.UserId is { } userId
            ? identityService.GetUserAsync(userId, cancellationToken)
            : Task.FromResult<AuthenticatedUser?>(null);
    }

    public Task LogoutAsync() => identityService.SignOutAsync();
}
