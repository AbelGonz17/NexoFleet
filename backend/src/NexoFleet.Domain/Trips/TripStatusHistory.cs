using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips;

public sealed class TripStatusHistory : Entity
{
    internal TripStatusHistory(
        Guid id,
        Guid tripId,
        Guid companyId,
        TripStatus? previousStatus,
        TripStatus currentStatus,
        string? notes,
        DateTimeOffset occurredAtUtc) : base(id)
    {
        TripId = tripId;
        CompanyId = companyId;
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        Notes = notes;
        OccurredAtUtc = occurredAtUtc;
    }

    private TripStatusHistory() { }

    public Guid TripId { get; private set; }
    public Guid CompanyId { get; private set; }
    public TripStatus? PreviousStatus { get; private set; }
    public TripStatus CurrentStatus { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
