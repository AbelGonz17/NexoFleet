using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IRouteScheduleRepository
{
    Task<RouteSchedule?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(
        Guid companyId,
        Guid routeId,
        CancellationToken cancellationToken = default);

    void Add(RouteSchedule routeSchedule);
}
