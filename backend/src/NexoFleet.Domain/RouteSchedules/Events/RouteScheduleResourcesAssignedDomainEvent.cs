using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.RouteSchedules.Events;

public sealed record RouteScheduleResourcesAssignedDomainEvent(
    Guid RouteScheduleId,
    Guid CompanyId,
    Guid AssignmentId,
    Guid EmployeeId,
    Guid? VehicleId,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    DateTimeOffset OccurredAt) : IDomainEvent;
