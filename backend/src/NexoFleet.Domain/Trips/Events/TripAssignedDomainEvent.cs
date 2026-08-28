using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips.Events;

public sealed record TripAssignedDomainEvent(
    Guid TripId,
    Guid CompanyId,
    Guid EmployeeId,
    Guid? VehicleId,
    DateTimeOffset OccurredAt) : IDomainEvent;
