namespace NexoFleet.Application.Authentication;

public sealed record LoginResult(LoginStatus Status, AuthenticatedUser? User = null)
{
    public static LoginResult Success(AuthenticatedUser user) => new(LoginStatus.Success, user);

    public static LoginResult Failed(LoginStatus status) => new(status);
}

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    LockedOut,
    Inactive
}
