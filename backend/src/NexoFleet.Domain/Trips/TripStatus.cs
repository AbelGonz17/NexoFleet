namespace NexoFleet.Domain.Trips;

public enum TripStatus
{
    PendingApproval = 1,
    Planned = 2,
    Assigned = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6,
    Rejected = 7
}
