using FluentValidation;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Domain.Common;

namespace NexoFleet.Application.Authentication;

public sealed class AuthService(
    IIdentityService identityService,
    ICurrentUser currentUser,
    IValidator<LoginRequest> loginValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator)
{
    public async Task<Result<AuthenticatedUser>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            return Result<AuthenticatedUser>.Failure(new ValidationError(errors));
        }

        return await identityService.PasswordSignInAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);
    }

    public async Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result<AuthenticatedUser>.Failure(AuthErrors.SessionNotFound);
        }

        var user = await identityService.GetUserAsync(userId, cancellationToken);
        return user is null
            ? Result<AuthenticatedUser>.Failure(AuthErrors.SessionNotFound)
            : Result<AuthenticatedUser>.Success(user);
    }

    public async Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AuthErrors.SessionNotFound);
        }

        var validationResult = await changePasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            return Result.Failure(new ValidationError(errors));
        }

        return await identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);
    }

    public async Task<Result> LogoutAsync()
    {
        await identityService.SignOutAsync();
        return Result.Success();
    }
}
