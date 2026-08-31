using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> GetByOwnerEmployeeIdAsync(
        Guid companyId,
        Guid ownerEmployeeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByLicensePlateAsync(
        Guid companyId,
        string licensePlate,
        Guid? excludingVehicleId = null,
        CancellationToken cancellationToken = default);

    void Add(Vehicle vehicle);
}
