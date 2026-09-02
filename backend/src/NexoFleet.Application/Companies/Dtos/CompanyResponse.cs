using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.Companies.Dtos;

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string TaxIdentification,
    string Country,
    string City,
    string Phone,
    string Email,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static CompanyResponse FromDomain(Company company) =>
        new(
            company.Id,
            company.Name.Value,
            company.TaxIdentification.Value,
            company.Address.Country,
            company.Address.City,
            company.Phone.Value,
            company.Email.Value,
            company.Status.ToString(),
            company.CreatedAtUtc,
            company.UpdatedAtUtc);
}
