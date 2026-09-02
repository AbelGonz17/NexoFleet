using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Employees;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class EmployeeRepository(ApplicationDbContext dbContext)
    : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Employees.SingleOrDefaultAsync(
            employee => employee.CompanyId == companyId && employee.Id == id,
            cancellationToken);

    public Task<Employee?> GetByUserIdAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        dbContext.Employees.SingleOrDefaultAsync(
            employee => employee.CompanyId == companyId && employee.UserId == userId,
            cancellationToken);

    public Task<bool> ExistsByEmployeeCodeAsync(
        Guid companyId,
        string employeeCode,
        Guid? excludingEmployeeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmployeeCode = NormalizeIdentifier(employeeCode);

        return dbContext.Employees.AnyAsync(
            employee =>
                employee.CompanyId == companyId &&
                employee.EmployeeCode == normalizedEmployeeCode &&
                (!excludingEmployeeId.HasValue || employee.Id != excludingEmployeeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByIdentityDocumentAsync(
        Guid companyId,
        string identityDocument,
        Guid? excludingEmployeeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentityDocument = NormalizeIdentifier(identityDocument);

        return dbContext.Employees.AnyAsync(
            employee =>
                employee.CompanyId == companyId &&
                employee.IdentityDocument == normalizedIdentityDocument &&
                (!excludingEmployeeId.HasValue || employee.Id != excludingEmployeeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(
        Guid companyId,
        string email,
        Guid? excludingEmployeeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Employees.AnyAsync(
            employee =>
                employee.CompanyId == companyId &&
                employee.Email == normalizedEmail &&
                (!excludingEmployeeId.HasValue || employee.Id != excludingEmployeeId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Employees
            .Where(employee => employee.CompanyId == companyId)
            .OrderBy(employee => employee.FullName)
            .ToListAsync(cancellationToken);

    public void Add(Employee employee) => dbContext.Employees.Add(employee);

    private static string NormalizeIdentifier(string value) =>
        value.Trim().ToUpperInvariant();
}
