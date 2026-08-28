using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class TripRepository(ApplicationDbContext dbContext) : ITripRepository
{
    public Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Trips
            .Include(trip => trip.Assignments)
            .Include(trip => trip.StatusHistory)
            .Include(trip => trip.Reviews)
            .Include(trip => trip.Incidents)
            .Include(trip => trip.Files)
            .SingleOrDefaultAsync(trip => trip.Id == id, cancellationToken);

    public Task<bool> ExistsByNumberAsync(Guid companyId, string tripNumber, CancellationToken cancellationToken = default)
    {
        var normalizedNumber = tripNumber.Trim().ToUpperInvariant();
        return dbContext.Trips.AnyAsync(
            trip => trip.CompanyId == companyId && trip.TripNumber == normalizedNumber,
            cancellationToken);
    }

    public void Add(Trip trip) => dbContext.Trips.Add(trip);
}
