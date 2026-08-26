namespace NexoFleet.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

