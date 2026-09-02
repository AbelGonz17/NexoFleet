using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record TripIncidentResponse(
    Guid Id,
    Guid TripId,
    Guid CompanyId,
    Guid ReportedByEmployeeId,
    string Severity,
    string Description,
    DateTimeOffset IncidentAtUtc,
    DateTimeOffset CreatedAtUtc)
{
    public static TripIncidentResponse FromDomain(TripIncident incident) =>
        new(
            incident.Id,
            incident.TripId,
            incident.CompanyId,
            incident.ReportedByEmployeeId,
            incident.Severity.ToString(),
            incident.Description,
            incident.IncidentAtUtc,
            incident.CreatedAtUtc);
}
