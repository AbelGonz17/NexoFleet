using NexoFleet.Domain.Employees;

namespace NexoFleet.Application.Employees.Dtos;

public sealed record EmployeeResponse(
    Guid Id,
    Guid CompanyId,
    Guid? UserId,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string FullName,
    string IdentityDocument,
    string Phone,
    string Email,
    DateOnly HireDate,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static EmployeeResponse FromDomain(Employee employee) =>
        new(
            employee.Id,
            employee.CompanyId,
            employee.UserId,
            employee.EmployeeCode.Value,
            employee.FullName.FirstName,
            employee.FullName.LastName,
            employee.FullName.ToString(),
            employee.IdentityDocument.Value,
            employee.Phone.Value,
            employee.Email.Value,
            employee.HireDate,
            employee.Status.ToString(),
            employee.CreatedAtUtc,
            employee.UpdatedAtUtc);
}
