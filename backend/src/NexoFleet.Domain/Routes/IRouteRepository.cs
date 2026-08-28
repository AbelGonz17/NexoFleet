namespace NexoFleet.Domain.Routes;

public interface IRouteRepository
{
    Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByRouteCodeAsync(
        Guid companyId,
        string routeCode,
        Guid? excludingRouteId = null,
        CancellationToken cancellationToken = default);

    void Add(Route route);
}
