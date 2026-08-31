using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class RouteScheduleRepository(ApplicationDbContext dbContext)
    : IRouteScheduleRepository
{
    public Task<RouteSchedule?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.RouteSchedules
            .Include(schedule => schedule.Days)
            .Include(schedule => schedule.Assignments)
            .SingleOrDefaultAsync(
                schedule => schedule.CompanyId == companyId && schedule.Id == id,
                cancellationToken);

    public async Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(
        Guid companyId,
        Guid routeId,
        CancellationToken cancellationToken = default) =>
        await dbContext.RouteSchedules
            .Include(schedule => schedule.Days)
            .Include(schedule => schedule.Assignments)
            .Where(schedule =>
                schedule.CompanyId == companyId &&
                schedule.RouteId == routeId)
            .ToListAsync(cancellationToken);

    public void Add(RouteSchedule routeSchedule) =>
        dbContext.RouteSchedules.Add(routeSchedule);
}
