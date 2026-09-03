using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Routes;
using NexoFleet.Application.Routes.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/routes")]
public sealed class RoutesController(RouteService routeService) : ControllerBase
{
    /// <summary>Lista las rutas de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RouteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.ListAsync(cancellationToken));

    /// <summary>Obtiene una ruta por su identificador.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.GetByIdAsync(id, cancellationToken));

    /// <summary>Crea una nueva ruta de transporte.</summary>
    /// <param name="request">Datos de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RouteResponse>> Create(
        [FromBody] CreateRouteRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.CreateAsync(request, cancellationToken));

    /// <summary>Actualiza los detalles y tarifas de una ruta.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="request">Datos modificados.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteResponse>> UpdateDetails(
        [FromRoute] Guid id,
        [FromBody] UpdateRouteDetailsRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.UpdateDetailsAsync(id, request, cancellationToken));

    /// <summary>Agrega una parada intermedia a la ruta.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="request">Datos de la parada.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/stops")]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteResponse>> AddStop(
        [FromRoute] Guid id,
        [FromBody] AddRouteStopRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.AddStopAsync(id, request, cancellationToken));

    /// <summary>Actualiza una parada intermedia.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="stopId">Identificador de la parada.</param>
    /// <param name="request">Datos de la parada a modificar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}/stops/{stopId:guid}")]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteResponse>> UpdateStop(
        [FromRoute] Guid id,
        [FromRoute] Guid stopId,
        [FromBody] UpdateRouteStopRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.UpdateStopAsync(id, stopId, request, cancellationToken));

    /// <summary>Reubica la secuencia de una parada intermedia.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="stopId">Identificador de la parada.</param>
    /// <param name="newSequence">Nueva posición en la secuencia.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/stops/{stopId:guid}/move/{newSequence:int}")]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteResponse>> MoveStop(
        [FromRoute] Guid id,
        [FromRoute] Guid stopId,
        [FromRoute] int newSequence,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.MoveStopAsync(id, stopId, newSequence, cancellationToken));

    /// <summary>Elimina una parada intermedia de la ruta.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="stopId">Identificador de la parada.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpDelete("{id:guid}/stops/{stopId:guid}")]
    [ProducesResponseType<RouteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RouteResponse>> RemoveStop(
        [FromRoute] Guid id,
        [FromRoute] Guid stopId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await routeService.RemoveStopAsync(id, stopId, cancellationToken));

    /// <summary>Activa una ruta inactiva.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await routeService.ActivateAsync(id, cancellationToken));

    /// <summary>Desactiva una ruta activa.</summary>
    /// <param name="id">Identificador de la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await routeService.DeactivateAsync(id, cancellationToken));
}
