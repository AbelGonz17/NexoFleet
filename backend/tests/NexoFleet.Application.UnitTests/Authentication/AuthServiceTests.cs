using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Authentication;
using NexoFleet.Domain.Common;

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
            SignInResult = Result<AuthenticatedUser>.Success(User)
        };
        var service = CreateService(identity);

        var result = await service.LoginAsync(
            new LoginRequest(" admin@nexofleet.test ", "ValidPassword123!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(User, result.Value);
        Assert.Equal("admin@nexofleet.test", identity.ReceivedEmail);
    }

    [Fact]
    public async Task LoginShouldRejectAnInvalidEmail()
    {
        var service = CreateService(new FakeIdentityService());

        var result = await service.LoginAsync(new LoginRequest("not-an-email", "password"));

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
    }

    [Fact]
    public async Task CurrentUserShouldBeLoadedFromTheAuthenticatedIdentifier()
    {
        var identity = new FakeIdentityService { User = User };
        var service = CreateService(identity, User.Id);

        var result = await service.GetCurrentUserAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(User, result.Value);
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
        public Result<AuthenticatedUser> SignInResult { get; init; } =
            Result<AuthenticatedUser>.Failure(AuthErrors.InvalidCredentials);

        public AuthenticatedUser? User { get; init; }

        public string? ReceivedEmail { get; private set; }

        public Guid? ReceivedUserId { get; private set; }

        public Task<Result<AuthenticatedUser>> PasswordSignInAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            ReceivedEmail = email;
            return Task.FromResult(SignInResult);
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
