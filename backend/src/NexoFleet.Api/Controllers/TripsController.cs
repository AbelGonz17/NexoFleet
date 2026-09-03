using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Trips;
using NexoFleet.Application.Trips.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/trips")]
public sealed class TripsController(TripService tripService) : ControllerBase
{
    /// <summary>Lista todos los viajes de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TripResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TripResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.ListAsync(cancellationToken));

    /// <summary>Obtiene el detalle completo de un viaje.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.GetByIdAsync(id, cancellationToken));

    /// <summary>Crea un nuevo viaje planificado.</summary>
    /// <param name="request">Datos del viaje planificado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("planned")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> CreatePlanned(
        [FromBody] CreatePlannedTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.CreatePlannedAsync(request, cancellationToken));

    /// <summary>Registra un viaje imprevisto reportado por un conductor.</summary>
    /// <param name="request">Datos del viaje imprevisto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("unexpected")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> SubmitUnexpected(
        [FromBody] SubmitUnexpectedTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.SubmitUnexpectedAsync(request, cancellationToken));

    /// <summary>Actualiza la planificación de un viaje planificado o pendiente de aprobación.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Datos modificados del plan de viaje.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}/plan")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> UpdatePlan(
        [FromRoute] Guid id,
        [FromBody] UpdateTripPlanRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.UpdatePlanAsync(id, request, cancellationToken));

    /// <summary>Aprueba administrativamente un viaje imprevisto.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Datos de aprobación y tarifas pactadas.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> Approve(
        [FromRoute] Guid id,
        [FromBody] ApproveTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.ApproveAsync(id, request, cancellationToken));

    /// <summary>Rechaza un viaje pendiente de aprobación.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Motivo del rechazo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> Reject(
        [FromRoute] Guid id,
        [FromBody] RejectTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.RejectAsync(id, request, cancellationToken));

    /// <summary>Asigna conductor y vehículo a un viaje.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Recursos a asignar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> Assign(
        [FromRoute] Guid id,
        [FromBody] AssignTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.AssignAsync(id, request, cancellationToken));

    /// <summary>Inicia la ejecución del viaje.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="employeeId">Identificador del empleado/conductor que inicia el viaje.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> Start(
        [FromRoute] Guid id,
        [FromQuery] Guid employeeId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.StartAsync(id, employeeId, cancellationToken));

    /// <summary>Completa un viaje en progreso registrando el monto final devengado.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="employeeId">Identificador del empleado/conductor que completa el viaje.</param>
    /// <param name="request">Monto final y moneda.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> Complete(
        [FromRoute] Guid id,
        [FromQuery] Guid employeeId,
        [FromBody] CompleteTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.CompleteAsync(id, employeeId, request, cancellationToken));

    /// <summary>Cancela un viaje no completado.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Motivo de la cancelación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> Cancel(
        [FromRoute] Guid id,
        [FromBody] CancelTripRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.CancelAsync(id, request, cancellationToken));

    /// <summary>Registra una incidencia durante el viaje.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Detalle de la incidencia.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/incidents")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> AddIncident(
        [FromRoute] Guid id,
        [FromBody] AddTripIncidentRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.AddIncidentAsync(id, request, cancellationToken));

    /// <summary>Adjunta un archivo o foto soporte al viaje.</summary>
    /// <param name="id">Identificador del viaje.</param>
    /// <param name="request">Metadatos del archivo adjunto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/files")]
    [ProducesResponseType<TripResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripResponse>> AddFile(
        [FromRoute] Guid id,
        [FromBody] AddTripFileRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await tripService.AddFileAsync(id, request, cancellationToken));
}
