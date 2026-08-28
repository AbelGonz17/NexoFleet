using NexoFleet.Domain.Auditing;

namespace NexoFleet.Domain.UnitTests.Auditing;

public sealed class AuditLogTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldProduceImmutableNormalizedRecord()
    {
        var result = AuditLog.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), " Trip.Completed ", " Trip ", Guid.NewGuid(),
            "{\"amount\":100}", " 127.0.0.1 ", " Browser ", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Trip.Completed", result.Value.Action);
        Assert.Equal("Trip", result.Value.EntityType);
        Assert.Equal("127.0.0.1", result.Value.IpAddress);
    }

    [Fact]
    public void CreateShouldRejectInvalidJsonData()
    {
        var result = AuditLog.Create(
            Guid.NewGuid(), null, Guid.NewGuid(), "Company.Created", "Company", Guid.NewGuid(),
            "not-json", null, null, Now);

        Assert.Equal(AuditLogErrors.DataInvalid, result.Error);
    }
}
