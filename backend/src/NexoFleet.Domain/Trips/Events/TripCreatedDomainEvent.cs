using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips.Events;

public sealed record TripCreatedDomainEvent(
    Guid TripId,
    Guid CompanyId,
    TripSource Source,
    DateTimeOffset OccurredAt) : IDomainEvent;
