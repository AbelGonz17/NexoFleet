using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record TripStatusHistoryResponse(
    Guid Id,
    Guid TripId,
    Guid CompanyId,
    string? PreviousStatus,
    string CurrentStatus,
    string? Notes,
    DateTimeOffset OccurredAtUtc)
{
    public static TripStatusHistoryResponse FromDomain(TripStatusHistory history) =>
        new(
            history.Id,
            history.TripId,
            history.CompanyId,
            history.PreviousStatus?.ToString(),
            history.CurrentStatus.ToString(),
            history.Notes,
            history.OccurredAtUtc);
}
