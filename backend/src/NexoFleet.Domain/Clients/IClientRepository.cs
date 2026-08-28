namespace NexoFleet.Domain.Clients;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid companyId,
        string clientCode,
        Guid? excludingClientId = null,
        CancellationToken cancellationToken = default);

    void Add(Client client);
}
