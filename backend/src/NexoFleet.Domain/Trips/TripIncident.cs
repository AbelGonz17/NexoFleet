using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips;

public sealed class TripIncident : Entity
{
    internal TripIncident(
        Guid id,
        Guid tripId,
        Guid companyId,
        Guid reportedByEmployeeId,
        TripIncidentSeverity severity,
        string description,
        DateTimeOffset incidentAtUtc,
        DateTimeOffset createdAtUtc) : base(id)
    {
        TripId = tripId;
        CompanyId = companyId;
        ReportedByEmployeeId = reportedByEmployeeId;
        Severity = severity;
        Description = description;
        IncidentAtUtc = incidentAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    private TripIncident() { }

    public Guid TripId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid ReportedByEmployeeId { get; private set; }
    public TripIncidentSeverity Severity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset IncidentAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
