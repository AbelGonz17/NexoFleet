using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Clients;
using NexoFleet.Application.Clients.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/clients")]
public sealed class ClientsController(ClientService clientService) : ControllerBase
{
    /// <summary>Lista los clientes de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClientResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await clientService.ListAsync(cancellationToken));

    /// <summary>Obtiene un cliente por su identificador.</summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await clientService.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra un nuevo cliente corporativo.</summary>
    /// <param name="request">Datos del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> Create(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await clientService.CreateAsync(request, cancellationToken));

    /// <summary>Actualiza el perfil de un cliente existente.</summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="request">Datos a modificar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await clientService.UpdateProfileAsync(id, request, cancellationToken));

    /// <summary>Activa un cliente previamente desactivado.</summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await clientService.ActivateAsync(id, cancellationToken));

    /// <summary>Desactiva un cliente.</summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await clientService.DeactivateAsync(id, cancellationToken));
}
