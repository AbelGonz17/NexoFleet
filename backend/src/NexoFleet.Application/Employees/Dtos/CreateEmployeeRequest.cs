namespace NexoFleet.Application.Employees.Dtos;

public sealed record CreateEmployeeRequest(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string IdentityDocument,
    string Phone,
    string Email,
    DateOnly HireDate);
