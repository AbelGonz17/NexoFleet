using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Authorization;
using NexoFleet.Application.Dashboards.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(
    IDashboardQueries dashboardQueries,
    ICurrentTenant currentTenant) : ControllerBase
{
    /// <summary>Obtiene los KPIs globales para el SuperAdmin.</summary>
    [HttpGet("superadmin/stats")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType<IReadOnlyList<GlobalKpiResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GlobalKpiResponse>>> GetGlobalStats(CancellationToken cancellationToken) =>
        Ok(await dashboardQueries.GetGlobalKpisAsync(cancellationToken));

    /// <summary>Obtiene las empresas con mayor actividad para el SuperAdmin.</summary>
    [HttpGet("superadmin/top-companies")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType<IReadOnlyList<TopCompanyResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TopCompanyResponse>>> GetTopCompanies(CancellationToken cancellationToken) =>
        Ok(await dashboardQueries.GetTopCompaniesAsync(cancellationToken));

    /// <summary>Obtiene el feed de auditoría en tiempo real para el SuperAdmin.</summary>
    [HttpGet("superadmin/audit-feed")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [ProducesResponseType<IReadOnlyList<AuditFeedResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditFeedResponse>>> GetAuditFeed(CancellationToken cancellationToken) =>
        Ok(await dashboardQueries.GetAuditFeedAsync(cancellationToken));

    /// <summary>Obtiene los estadísticos y KPIs de la empresa autenticada.</summary>
    [HttpGet("company/stats")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType<CompanyStatsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CompanyStatsResponse>> GetCompanyStats(CancellationToken cancellationToken)
    {
        var companyId = currentTenant.CompanyId;
        if (!companyId.HasValue)
        {
            return Forbid();
        }

        return Ok(await dashboardQueries.GetCompanyStatsAsync(companyId.Value, cancellationToken));
    }

    /// <summary>Obtiene los viajes recientes de la empresa autenticada.</summary>
    [HttpGet("company/recent-trips")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType<IReadOnlyList<RecentTripResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<RecentTripResponse>>> GetRecentTrips(CancellationToken cancellationToken)
    {
        var companyId = currentTenant.CompanyId;
        if (!companyId.HasValue)
        {
            return Forbid();
        }

        return Ok(await dashboardQueries.GetRecentTripsAsync(companyId.Value, cancellationToken));
    }
}
