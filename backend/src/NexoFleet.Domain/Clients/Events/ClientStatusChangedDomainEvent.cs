using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Clients.Events;

public sealed record ClientStatusChangedDomainEvent(
    Guid ClientId,
    Guid CompanyId,
    ClientStatus PreviousStatus,
    ClientStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
