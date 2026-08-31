using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository(ApplicationDbContext dbContext)
    : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Vehicles
            .Include(vehicle => vehicle.Documents)
            .SingleOrDefaultAsync(
                vehicle => vehicle.CompanyId == companyId && vehicle.Id == id,
                cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetByOwnerEmployeeIdAsync(
        Guid companyId,
        Guid ownerEmployeeId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Vehicles
            .Include(vehicle => vehicle.Documents)
            .Where(vehicle =>
                vehicle.CompanyId == companyId &&
                vehicle.OwnerEmployeeId == ownerEmployeeId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByLicensePlateAsync(
        Guid companyId,
        string licensePlate,
        Guid? excludingVehicleId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedLicensePlate = licensePlate.Trim().ToUpperInvariant();

        return dbContext.Vehicles.AnyAsync(
            vehicle =>
                vehicle.CompanyId == companyId &&
                vehicle.LicensePlate == normalizedLicensePlate &&
                (!excludingVehicleId.HasValue || vehicle.Id != excludingVehicleId.Value),
            cancellationToken);
    }

    public void Add(Vehicle vehicle) => dbContext.Vehicles.Add(vehicle);
}
