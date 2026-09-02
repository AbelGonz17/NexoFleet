using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.RouteSchedules.Dtos;

public sealed record RouteScheduleAssignmentResponse(
    Guid Id,
    Guid RouteScheduleId,
    Guid CompanyId,
    Guid EmployeeId,
    Guid? VehicleId,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static RouteScheduleAssignmentResponse FromDomain(RouteScheduleAssignment assignment) =>
        new(
            assignment.Id,
            assignment.RouteScheduleId,
            assignment.CompanyId,
            assignment.EmployeeId,
            assignment.VehicleId,
            assignment.ValidFrom,
            assignment.ValidUntil,
            assignment.CreatedAtUtc,
            assignment.UpdatedAtUtc);
}
