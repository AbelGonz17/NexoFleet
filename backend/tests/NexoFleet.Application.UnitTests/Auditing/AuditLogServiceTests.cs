using NexoFleet.Application.Auditing;
using NexoFleet.Application.Auditing.Dtos;
using NexoFleet.Application.Auditing.Validators;
using NexoFleet.Application.UnitTests.Fakes;

namespace NexoFleet.Application.UnitTests.Auditing;

public sealed class AuditLogServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task LogAsyncShouldCreateAuditLogWhenValid()
    {
        var repo = new FakeAuditLogRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var user = new FakeCurrentUser(UserId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = new AuditLogService(repo, tenant, user, uow, clock, new CreateAuditLogRequestValidator());

        var request = new CreateAuditLogRequest(
            "TripCreated",
            "Trip",
            Guid.NewGuid(),
            "{\"tripNumber\":\"TRIP-100\"}",
            "192.168.1.1",
            "Mozilla/5.0");

        var result = await service.LogAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("TripCreated", result.Value.Action);
        Assert.Equal("Trip", result.Value.EntityType);
        Assert.Single(repo.Logs);
        Assert.Equal(1, uow.SaveChangesCalls);
    }
}
