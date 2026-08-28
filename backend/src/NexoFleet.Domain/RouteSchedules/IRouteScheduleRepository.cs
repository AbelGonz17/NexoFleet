namespace NexoFleet.Domain.RouteSchedules;

public interface IRouteScheduleRepository
{
    Task<RouteSchedule?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(
        Guid routeId,
        CancellationToken cancellationToken = default);

    void Add(RouteSchedule routeSchedule);
}
