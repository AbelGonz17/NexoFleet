using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IRouteRepository
{
    Task<Route?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByRouteCodeAsync(
        Guid companyId,
        string routeCode,
        Guid? excludingRouteId = null,
        CancellationToken cancellationToken = default);

    void Add(Route route);
}
