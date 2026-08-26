using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.UnitTests.Common;

public sealed class EntityTests
{
    [Fact]
    public void AggregateRootShouldExposeItsIdentifier()
    {
        var id = Guid.NewGuid();

        var aggregate = new TestAggregate(id);

        Assert.Equal(id, aggregate.Id);
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot(id);
}
