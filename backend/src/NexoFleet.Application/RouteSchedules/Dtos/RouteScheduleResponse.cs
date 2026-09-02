using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.RouteSchedules.Dtos;

public sealed record RouteScheduleResponse(
    Guid Id,
    Guid CompanyId,
    Guid RouteId,
    string Shift,
    TimeOnly StartTime,
    TimeOnly? EndTime,
    IReadOnlyList<DayOfWeek> Days,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    decimal? DefaultAmount,
    string? DefaultCurrency,
    string Status,
    IReadOnlyList<RouteScheduleAssignmentResponse> Assignments,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static RouteScheduleResponse FromDomain(RouteSchedule schedule) =>
        new(
            schedule.Id,
            schedule.CompanyId,
            schedule.RouteId,
            schedule.Shift.ToString(),
            schedule.StartTime,
            schedule.EndTime,
            schedule.Days.Select(d => d.DayOfWeek).OrderBy(d => d).ToArray(),
            schedule.EffectiveFrom,
            schedule.EffectiveUntil,
            schedule.DefaultAmount,
            schedule.DefaultCurrency,
            schedule.Status.ToString(),
            schedule.Assignments.OrderByDescending(a => a.ValidFrom).Select(RouteScheduleAssignmentResponse.FromDomain).ToArray(),
            schedule.CreatedAtUtc,
            schedule.UpdatedAtUtc);
}
