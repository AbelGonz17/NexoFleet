namespace NexoFleet.Application.Dashboards.Dtos;

public sealed record AuditFeedResponse(
    string Id,
    string Action,
    string Description,
    string User,
    string Time,
    string Severity
);
