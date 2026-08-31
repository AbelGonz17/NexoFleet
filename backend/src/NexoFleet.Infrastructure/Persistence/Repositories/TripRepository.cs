using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class TripRepository(ApplicationDbContext dbContext) : ITripRepository
{
    public Task<Trip?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Trips
            .Include(trip => trip.Assignments)
            .Include(trip => trip.StatusHistory)
            .Include(trip => trip.Reviews)
            .Include(trip => trip.Incidents)
            .Include(trip => trip.Files)
            .SingleOrDefaultAsync(
                trip => trip.CompanyId == companyId && trip.Id == id,
                cancellationToken);

    public Task<bool> ExistsByNumberAsync(Guid companyId, string tripNumber, CancellationToken cancellationToken = default)
    {
        var normalizedNumber = tripNumber.Trim().ToUpperInvariant();
        return dbContext.Trips.AnyAsync(
            trip => trip.CompanyId == companyId && trip.TripNumber == normalizedNumber,
            cancellationToken);
    }

    public Task<bool> HasInProgressTripForVehicleAsync(
        Guid companyId,
        Guid vehicleId,
        Guid? excludingTripId = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            from assignment in dbContext.Set<TripAssignment>()
            join trip in dbContext.Trips
                on new { assignment.CompanyId, assignment.TripId }
                equals new { trip.CompanyId, TripId = trip.Id }
            where assignment.CompanyId == companyId &&
                assignment.VehicleId == vehicleId &&
                !assignment.EndedAtUtc.HasValue &&
                trip.Status == TripStatus.InProgress &&
                (!excludingTripId.HasValue || trip.Id != excludingTripId.Value)
            select assignment;

        return query.AnyAsync(cancellationToken);
    }

    public void Add(Trip trip) => dbContext.Trips.Add(trip);
}
