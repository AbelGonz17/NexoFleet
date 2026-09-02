using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class RouteRepository(ApplicationDbContext dbContext)
    : IRouteRepository
{
    public Task<Route?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Routes
            .Include(route => route.Stops)
            .SingleOrDefaultAsync(
                route => route.CompanyId == companyId && route.Id == id,
                cancellationToken);

    public Task<bool> ExistsByRouteCodeAsync(
        Guid companyId,
        string routeCode,
        Guid? excludingRouteId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedRouteCode = routeCode.Trim().ToUpperInvariant();

        return dbContext.Routes.AnyAsync(
            route =>
                route.CompanyId == companyId &&
                route.RouteCode == normalizedRouteCode &&
                (!excludingRouteId.HasValue || route.Id != excludingRouteId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Route>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Routes
            .Include(route => route.Stops)
            .Where(route => route.CompanyId == companyId)
            .OrderBy(route => route.Name)
            .ToListAsync(cancellationToken);

    public void Add(Route route) => dbContext.Routes.Add(route);
}
