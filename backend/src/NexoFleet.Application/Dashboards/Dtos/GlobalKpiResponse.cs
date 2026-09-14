namespace NexoFleet.Application.Dashboards.Dtos;

public sealed record GlobalKpiResponse(
    string Name,
    string Value,
    string Subtitle,
    string Change,
    string Trend,
    string IconName
);
