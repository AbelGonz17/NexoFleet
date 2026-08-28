using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Clients.Events;

public sealed record ClientCreatedDomainEvent(
    Guid ClientId,
    Guid CompanyId,
    DateTimeOffset OccurredAt) : IDomainEvent;
