using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Notifications;
using NexoFleet.Application.Notifications.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(NotificationService notificationService) : ControllerBase
{
    /// <summary>Obtiene las notificaciones del usuario autenticado.</summary>
    [HttpGet("my")]
    [ProducesResponseType<IReadOnlyList<NotificationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetMyNotifications(CancellationToken cancellationToken) =>
        this.ToActionResult(await notificationService.GetMyNotificationsAsync(cancellationToken));

    /// <summary>Lista todas las notificaciones de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await notificationService.ListAsync(cancellationToken));

    /// <summary>Obtiene una notificación por su identificador.</summary>
    /// <param name="id">Identificador de la notificación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<NotificationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await notificationService.GetByIdAsync(id, cancellationToken));

    /// <summary>Envía una nueva notificación.</summary>
    /// <param name="request">Datos de la notificación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<NotificationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> Create(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await notificationService.CreateAsync(request, cancellationToken));

    /// <summary>Marca una notificación como leída.</summary>
    /// <param name="id">Identificador de la notificación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await notificationService.MarkAsReadAsync(id, cancellationToken));

    /// <summary>Archiva una notificación.</summary>
    /// <param name="id">Identificador de la notificación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await notificationService.ArchiveAsync(id, cancellationToken));
}
