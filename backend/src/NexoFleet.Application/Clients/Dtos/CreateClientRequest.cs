namespace NexoFleet.Application.Clients.Dtos;

public sealed record CreateClientRequest(
    string ClientCode,
    string Name,
    string? TaxIdentification = null,
    string? ContactName = null,
    string? Phone = null,
    string? Email = null);
