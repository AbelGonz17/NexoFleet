using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Auditing;
using NexoFleet.Application.Auditing.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/audit-logs")]
public sealed class AuditLogsController(AuditLogService auditLogService) : ControllerBase
{
    /// <summary>Lista los registros de auditoría de la empresa o sistema.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AuditLogResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLogResponse>>> Search(
        [FromQuery] string? search = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? severity = null,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await auditLogService.SearchAsync(search, entityType, severity, cancellationToken));

    /// <summary>Obtiene estadísticas de auditoría global.</summary>
    [HttpGet("stats")]
    [ProducesResponseType<AuditLogStatsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLogStatsResponse>> GetStats(CancellationToken cancellationToken) =>
        this.ToActionResult(await auditLogService.GetStatsAsync(cancellationToken));

    /// <summary>Obtiene un registro de auditoría por su identificador.</summary>
    /// <param name="id">Identificador del log de auditoría.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AuditLogResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await auditLogService.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra una acción auditada en el sistema.</summary>
    /// <param name="request">Detalle de la acción auditada.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<AuditLogResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuditLogResponse>> Log(
        [FromBody] CreateAuditLogRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await auditLogService.LogAsync(request, cancellationToken));
}
