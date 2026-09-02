using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class CompanyRepository(ApplicationDbContext dbContext)
    : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Companies.SingleOrDefaultAsync(
            company => company.Id == id,
            cancellationToken);

    public Task<bool> ExistsByTaxIdentificationAsync(
        string taxIdentification,
        Guid? excludingCompanyId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedTaxIdentification = taxIdentification.Trim().ToUpperInvariant();

        return dbContext.Companies.AnyAsync(
            company =>
                company.TaxIdentification == normalizedTaxIdentification &&
                (!excludingCompanyId.HasValue || company.Id != excludingCompanyId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> ListAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Companies
            .OrderBy(company => company.Name)
            .ToListAsync(cancellationToken);

    public void Add(Company company) => dbContext.Companies.Add(company);
}
