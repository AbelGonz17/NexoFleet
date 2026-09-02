using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;

namespace NexoFleet.Application.UnitTests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await operation(cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        var result = await operation(cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return result;
    }
}

public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}

public sealed class FakeCurrentTenant(Guid? companyId) : ICurrentTenant
{
    public Guid? CompanyId { get; set; } = companyId;
    public bool IsAvailable => CompanyId.HasValue;
}

public sealed class FakeCurrentUser(Guid? userId, string? role = null) : ICurrentUser
{
    public Guid? UserId { get; set; } = userId;
    public string? Role { get; set; } = role;
    public bool IsAuthenticated => UserId.HasValue;
}
