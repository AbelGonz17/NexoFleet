using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes.Events;

public sealed record RouteStatusChangedDomainEvent(
    Guid RouteId,
    Guid CompanyId,
    RouteStatus PreviousStatus,
    RouteStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
