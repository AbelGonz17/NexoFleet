using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.RouteSchedules.Events;

public sealed record RouteScheduleStatusChangedDomainEvent(
    Guid RouteScheduleId,
    Guid CompanyId,
    RouteScheduleStatus PreviousStatus,
    RouteScheduleStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
