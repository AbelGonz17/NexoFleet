namespace NexoFleet.Application.RouteSchedules.Dtos;

public sealed record AssignScheduleResourcesRequest(
    Guid EmployeeId,
    DateOnly ValidFrom,
    Guid? VehicleId = null,
    DateOnly? ValidUntil = null);
