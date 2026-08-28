using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes.Events;

public sealed record RouteCreatedDomainEvent(
    Guid RouteId,
    Guid CompanyId,
    DateTimeOffset OccurredAt) : IDomainEvent;
