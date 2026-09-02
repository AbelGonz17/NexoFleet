using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record TripAssignmentResponse(
    Guid Id,
    Guid TripId,
    Guid CompanyId,
    Guid EmployeeId,
    Guid? VehicleId,
    Guid AssignedByUserId,
    DateTimeOffset AssignedAtUtc,
    DateTimeOffset? EndedAtUtc)
{
    public static TripAssignmentResponse FromDomain(TripAssignment assignment) =>
        new(
            assignment.Id,
            assignment.TripId,
            assignment.CompanyId,
            assignment.EmployeeId,
            assignment.VehicleId,
            assignment.AssignedByUserId,
            assignment.AssignedAtUtc,
            assignment.EndedAtUtc);
}
