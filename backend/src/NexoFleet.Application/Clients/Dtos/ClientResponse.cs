using NexoFleet.Domain.Clients;

namespace NexoFleet.Application.Clients.Dtos;

public sealed record ClientResponse(
    Guid Id,
    Guid CompanyId,
    string ClientCode,
    string Name,
    string? TaxIdentification,
    string? ContactName,
    string? Phone,
    string? Email,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static ClientResponse FromDomain(Client client) =>
        new(
            client.Id,
            client.CompanyId,
            client.ClientCode.Value,
            client.Name.Value,
            client.TaxIdentification?.Value,
            client.ContactName?.Value,
            client.Phone?.Value,
            client.Email?.Value,
            client.Status.ToString(),
            client.CreatedAtUtc,
            client.UpdatedAtUtc);
}
