using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.RouteSchedules.Dtos;

public sealed record CreateRouteScheduleRequest(
    Guid RouteId,
    RouteShift Shift,
    TimeOnly StartTime,
    IReadOnlyList<DayOfWeek> Days,
    DateOnly EffectiveFrom,
    TimeOnly? EndTime = null,
    DateOnly? EffectiveUntil = null,
    decimal? DefaultAmount = null,
    string? DefaultCurrency = null);
