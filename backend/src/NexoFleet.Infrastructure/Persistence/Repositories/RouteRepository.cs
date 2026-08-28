using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class RouteRepository(ApplicationDbContext dbContext)
    : IRouteRepository
{
    public Task<Route?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Routes
            .Include(route => route.Stops)
            .SingleOrDefaultAsync(route => route.Id == id, cancellationToken);

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

    public void Add(Route route) => dbContext.Routes.Add(route);
}
