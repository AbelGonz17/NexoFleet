using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNumberAsync(
        Guid companyId,
        string tripNumber,
        CancellationToken cancellationToken = default);

    Task<bool> HasInProgressTripForVehicleAsync(
        Guid companyId,
        Guid vehicleId,
        Guid? excludingTripId = null,
        CancellationToken cancellationToken = default);

    void Add(Trip trip);
}
