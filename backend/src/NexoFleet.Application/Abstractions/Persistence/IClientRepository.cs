using NexoFleet.Domain.Clients;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid companyId,
        string clientCode,
        Guid? excludingClientId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Client>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    void Add(Client client);
}
