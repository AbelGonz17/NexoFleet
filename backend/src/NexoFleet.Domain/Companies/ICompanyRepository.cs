namespace NexoFleet.Domain.Companies;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTaxIdentificationAsync(
        string taxIdentification,
        Guid? excludingCompanyId = null,
        CancellationToken cancellationToken = default);

    void Add(Company company);
}
