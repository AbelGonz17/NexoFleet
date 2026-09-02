using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record AddTripIncidentRequest(
    Guid ReportedByEmployeeId,
    TripIncidentSeverity Severity,
    string Description,
    DateTimeOffset IncidentAtUtc);
