using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByTaxIdentificationAsync(
        string taxIdentification,
        Guid? excludingCompanyId = null,
        CancellationToken cancellationToken = default);

    void Add(Company company);
}
