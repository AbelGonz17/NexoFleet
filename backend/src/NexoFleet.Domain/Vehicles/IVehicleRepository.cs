namespace NexoFleet.Domain.Vehicles;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> GetByOwnerEmployeeIdAsync(
        Guid ownerEmployeeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByLicensePlateAsync(
        Guid companyId,
        string licensePlate,
        Guid? excludingVehicleId = null,
        CancellationToken cancellationToken = default);

    void Add(Vehicle vehicle);
}
