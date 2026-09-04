namespace NexoFleet.Application.Companies.Dtos;

public sealed record CreateCompanyAdminRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);
