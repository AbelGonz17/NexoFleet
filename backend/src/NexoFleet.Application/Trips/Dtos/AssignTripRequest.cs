namespace NexoFleet.Application.Trips.Dtos;

public sealed record AssignTripRequest(
    Guid EmployeeId,
    Guid? VehicleId = null);
