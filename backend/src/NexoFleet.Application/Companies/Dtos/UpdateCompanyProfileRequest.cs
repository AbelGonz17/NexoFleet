namespace NexoFleet.Application.Companies.Dtos;

public sealed record UpdateCompanyProfileRequest(
    string Name,
    string TaxIdentification,
    string Country,
    string City,
    string Phone,
    string Email);
