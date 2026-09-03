using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Vehicles;
using NexoFleet.Application.Vehicles.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/vehicles")]
public sealed class VehiclesController(VehicleService vehicleService) : ControllerBase
{
    /// <summary>Lista todos los vehículos de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<VehicleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await vehicleService.ListAsync(cancellationToken));

    /// <summary>Obtiene el detalle de un vehículo por su identificador.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await vehicleService.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra un vehículo propio de la empresa.</summary>
    /// <param name="request">Datos del vehículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("company")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleResponse>> RegisterCompanyVehicle(
        [FromBody] RegisterCompanyVehicleRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await vehicleService.RegisterCompanyVehicleAsync(request, cancellationToken));

    /// <summary>Registra un vehículo perteneciente a un empleado o conductor.</summary>
    /// <param name="request">Datos del vehículo y propietario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("employee")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleResponse>> RegisterEmployeeVehicle(
        [FromBody] RegisterEmployeeVehicleRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await vehicleService.RegisterEmployeeVehicleAsync(request, cancellationToken));

    /// <summary>Actualiza las características de un vehículo.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="request">Datos actualizados.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleResponse>> UpdateDetails(
        [FromRoute] Guid id,
        [FromBody] UpdateVehicleDetailsRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await vehicleService.UpdateDetailsAsync(id, request, cancellationToken));

    /// <summary>Aprueba un vehículo de empleado para operar.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await vehicleService.ApproveAsync(id, cancellationToken));

    /// <summary>Rechaza un vehículo de empleado.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="request">Motivo del rechazo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id,
        [FromBody] RejectVehicleRequest request,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await vehicleService.RejectAsync(id, request, cancellationToken));

    /// <summary>Adjunta un documento a la ficha del vehículo.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="request">Datos del documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/documents")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> AddDocument(
        [FromRoute] Guid id,
        [FromBody] AddVehicleDocumentRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await vehicleService.AddDocumentAsync(id, request, cancellationToken));

    /// <summary>Elimina un documento del vehículo.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="documentId">Identificador del documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDocument(
        [FromRoute] Guid id,
        [FromRoute] Guid documentId,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await vehicleService.RemoveDocumentAsync(id, documentId, cancellationToken));

    /// <summary>Envía un vehículo a mantenimiento.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/maintenance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SendToMaintenance(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await vehicleService.SendToMaintenanceAsync(id, cancellationToken));

    /// <summary>Retorna un vehículo de mantenimiento al estado operativo.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/operational")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReturnToOperational(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await vehicleService.ReturnToOperationalAsync(id, cancellationToken));

    /// <summary>Retira definitivamente un vehículo de la flota.</summary>
    /// <param name="id">Identificador del vehículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/retire")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Retire(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await vehicleService.RetireAsync(id, cancellationToken));
}
