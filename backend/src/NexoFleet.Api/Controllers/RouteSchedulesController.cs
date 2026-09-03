using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.RouteSchedules;
using NexoFleet.Application.RouteSchedules.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/route-schedules")]
public sealed class RouteSchedulesController(RouteScheduleService scheduleService) : ControllerBase
{
    /// <summary>Lista las programaciones de rutas de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RouteScheduleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteScheduleResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.ListAsync(cancellationToken));

    /// <summary>Obtiene una programación de ruta por su identificador.</summary>
    /// <param name="id">Identificador de la programación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RouteScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteScheduleResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.GetByIdAsync(id, cancellationToken));

    /// <summary>Lista las programaciones asociadas a una ruta específica.</summary>
    /// <param name="routeId">Identificador de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("by-route/{routeId:guid}")]
    [ProducesResponseType<IReadOnlyList<RouteScheduleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteScheduleResponse>>> GetByRouteId(
        [FromRoute] Guid routeId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.GetByRouteIdAsync(routeId, cancellationToken));

    /// <summary>Crea una nueva programación para una ruta.</summary>
    /// <param name="request">Datos de la programación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<RouteScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteScheduleResponse>> Create(
        [FromBody] CreateRouteScheduleRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.CreateAsync(request, cancellationToken));

    /// <summary>Configura la recurrencia, horario y turnos de una programación.</summary>
    /// <param name="id">Identificador de la programación.</param>
    /// <param name="request">Configuración de recurrencia.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}/recurrence")]
    [ProducesResponseType<RouteScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteScheduleResponse>> ConfigureRecurrence(
        [FromRoute] Guid id,
        [FromBody] ConfigureRouteScheduleRecurrenceRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.ConfigureRecurrenceAsync(id, request, cancellationToken));

    /// <summary>Asigna conductor y vehículo a la programación de ruta.</summary>
    /// <param name="id">Identificador de la programación.</param>
    /// <param name="request">Detalle de la asignación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/assignments")]
    [ProducesResponseType<RouteScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RouteScheduleResponse>> AssignResources(
        [FromRoute] Guid id,
        [FromBody] AssignScheduleResourcesRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.AssignResourcesAsync(id, request, cancellationToken));

    /// <summary>Finaliza la asignación de recursos activa en la programación.</summary>
    /// <param name="id">Identificador de la programación.</param>
    /// <param name="request">Fecha de finalización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/assignments/end-current")]
    [ProducesResponseType<RouteScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RouteScheduleResponse>> EndCurrentAssignment(
        [FromRoute] Guid id,
        [FromBody] EndCurrentScheduleAssignmentRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await scheduleService.EndCurrentAssignmentAsync(id, request, cancellationToken));

    /// <summary>Activa una programación inactiva.</summary>
    /// <param name="id">Identificador de la programación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await scheduleService.ActivateAsync(id, cancellationToken));

    /// <summary>Desactiva una programación activa.</summary>
    /// <param name="id">Identificador de la programación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await scheduleService.DeactivateAsync(id, cancellationToken));
}
