using NexoFleet.Domain.Employees;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Employee?> GetByUserIdAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmployeeCodeAsync(
        Guid companyId,
        string employeeCode,
        Guid? excludingEmployeeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdentityDocumentAsync(
        Guid companyId,
        string identityDocument,
        Guid? excludingEmployeeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        Guid companyId,
        string email,
        Guid? excludingEmployeeId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Employee>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    void Add(Employee employee);
}
