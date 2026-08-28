using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.RouteSchedules.Events;

public sealed record RouteScheduleRecurrenceChangedDomainEvent(
    Guid RouteScheduleId,
    Guid CompanyId,
    Guid RouteId,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    DateTimeOffset OccurredAt) : IDomainEvent;
