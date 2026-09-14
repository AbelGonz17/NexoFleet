namespace NexoFleet.Application.Dashboards.Dtos;

public sealed record CompanyKpiResponse(
    string Name,
    string Value,
    string Change,
    string Trend,
    string IconName
);

public sealed record FleetStatusResponse(
    int EnRuta,
    int Disponibles,
    int EnMantenimiento
);

public sealed record CompanyStatsResponse(
    IReadOnlyList<CompanyKpiResponse> Kpis,
    FleetStatusResponse FleetStatus
);
