using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips.Events;

public sealed record TripCompletedDomainEvent(
    Guid TripId,
    Guid CompanyId,
    Guid EmployeeId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt) : IDomainEvent;
