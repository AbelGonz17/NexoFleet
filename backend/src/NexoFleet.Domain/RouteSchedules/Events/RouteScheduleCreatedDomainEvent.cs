using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.RouteSchedules.Events;

public sealed record RouteScheduleCreatedDomainEvent(
    Guid RouteScheduleId,
    Guid CompanyId,
    Guid RouteId,
    DateTimeOffset OccurredAt) : IDomainEvent;
