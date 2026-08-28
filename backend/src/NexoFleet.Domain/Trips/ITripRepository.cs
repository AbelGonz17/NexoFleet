namespace NexoFleet.Domain.Trips;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAsync(Guid companyId, string tripNumber, CancellationToken cancellationToken = default);
    void Add(Trip trip);
}
