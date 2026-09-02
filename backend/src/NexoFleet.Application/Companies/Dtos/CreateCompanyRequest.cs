namespace NexoFleet.Application.Companies.Dtos;

public sealed record CreateCompanyRequest(
    string Name,
    string TaxIdentification,
    string Country,
    string City,
    string Phone,
    string Email);
