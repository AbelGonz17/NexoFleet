using Microsoft.EntityFrameworkCore;
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

    public void Add(Company company) => dbContext.Companies.Add(company);
}
