using FluentValidation;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Authentication;

namespace NexoFleet.Application.UnitTests.Authentication;

public sealed class AuthServiceTests
{
    private static readonly AuthenticatedUser User = new(
        Guid.NewGuid(),
        "admin@nexofleet.test",
        "Nexo",
        "Admin",
        null,
        ["SuperAdmin"]);

    [Fact]
    public async Task LoginShouldReturnTheAuthenticatedUser()
    {
        var identity = new FakeIdentityService
        {
            LoginResult = LoginResult.Success(User)
        };
        var service = CreateService(identity);

        var result = await service.LoginAsync(
            new LoginRequest(" admin@nexofleet.test ", "ValidPassword123!"));

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.Equal(User, result.User);
        Assert.Equal("admin@nexofleet.test", identity.ReceivedEmail);
    }

    [Fact]
    public async Task LoginShouldRejectAnInvalidEmail()
    {
        var service = CreateService(new FakeIdentityService());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginAsync(new LoginRequest("not-an-email", "password")));
    }

    [Fact]
    public async Task CurrentUserShouldBeLoadedFromTheAuthenticatedIdentifier()
    {
        var identity = new FakeIdentityService { User = User };
        var service = CreateService(identity, User.Id);

        var result = await service.GetCurrentUserAsync();

        Assert.Equal(User, result);
        Assert.Equal(User.Id, identity.ReceivedUserId);
    }

    private static AuthService CreateService(
        FakeIdentityService identity,
        Guid? currentUserId = null)
    {
        return new AuthService(
            identity,
            new FakeCurrentUser(currentUserId),
            new LoginRequestValidator());
    }

    private sealed class FakeCurrentUser(Guid? userId) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;

        public string? Role => null;

        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class FakeIdentityService : IIdentityService
    {
        public LoginResult LoginResult { get; init; } = LoginResult.Failed(LoginStatus.InvalidCredentials);

        public AuthenticatedUser? User { get; init; }

        public string? ReceivedEmail { get; private set; }

        public Guid? ReceivedUserId { get; private set; }

        public Task<LoginResult> PasswordSignInAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            ReceivedEmail = email;
            return Task.FromResult(LoginResult);
        }

        public Task<AuthenticatedUser?> GetUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            ReceivedUserId = userId;
            return Task.FromResult(User);
        }

        public Task SignOutAsync() => Task.CompletedTask;
    }
}
