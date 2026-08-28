using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips.Events;

public sealed record TripStatusChangedDomainEvent(
    Guid TripId,
    Guid CompanyId,
    TripStatus PreviousStatus,
    TripStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
