using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Clients;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class ClientRepository(ApplicationDbContext dbContext) : IClientRepository
{
    public Task<Client?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Clients.SingleOrDefaultAsync(
            client => client.CompanyId == companyId && client.Id == id,
            cancellationToken);

    public Task<bool> ExistsByCodeAsync(
        Guid companyId,
        string clientCode,
        Guid? excludingClientId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = clientCode.Trim().ToUpperInvariant();
        return dbContext.Clients.AnyAsync(
            client => client.CompanyId == companyId &&
                client.ClientCode == normalizedCode &&
                (!excludingClientId.HasValue || client.Id != excludingClientId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Clients
            .Where(client => client.CompanyId == companyId)
            .OrderBy(client => client.Name)
            .ToListAsync(cancellationToken);

    public void Add(Client client) => dbContext.Clients.Add(client);
}
