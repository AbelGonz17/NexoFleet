using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Dtos;

public sealed record RegisterEmployeeVehicleRequest(
    Guid OwnerEmployeeId,
    string LicensePlate,
    string Make,
    string Model,
    int ManufactureYear,
    string? Color,
    VehicleType Type,
    int? PassengerCapacity);
