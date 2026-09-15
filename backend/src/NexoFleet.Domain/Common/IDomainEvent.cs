using MediatR;

namespace NexoFleet.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAt { get; }
}

