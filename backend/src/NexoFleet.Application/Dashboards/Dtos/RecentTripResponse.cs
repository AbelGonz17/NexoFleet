namespace NexoFleet.Application.Dashboards.Dtos;

public sealed record RecentTripResponse(
    string Number,
    string Route,
    string Driver,
    string Vehicle,
    string Status,
    string Time
);
