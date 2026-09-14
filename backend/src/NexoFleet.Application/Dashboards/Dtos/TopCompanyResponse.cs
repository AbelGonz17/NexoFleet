namespace NexoFleet.Application.Dashboards.Dtos;

public sealed record TopCompanyResponse(
    string Id,
    string Name,
    string Ruc,
    int FleetCount,
    int TripsThisMonth,
    string Status,
    string AdminEmail
);
