using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Payments;
using NexoFleet.Application.Payments.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/payment-reports")]
public sealed class PaymentReportsController(PaymentReportService reportService) : ControllerBase
{
    /// <summary>Lista las relaciones de pago de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PaymentReportResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentReportResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.ListAsync(cancellationToken));

    /// <summary>Obtiene una relación de pago por su identificador.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentReportResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.GetByIdAsync(id, cancellationToken));

    /// <summary>Lista las relaciones de pago de un periodo específico.</summary>
    /// <param name="periodId">Identificador del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("by-period/{periodId:guid}")]
    [ProducesResponseType<IReadOnlyList<PaymentReportResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentReportResponse>>> ListByPeriod(
        [FromRoute] Guid periodId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.ListByPeriodIdAsync(periodId, cancellationToken));

    /// <summary>Crea una relación de pago en borrador para un empleado en un periodo.</summary>
    /// <param name="request">Datos del reporte a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> Create(
        [FromBody] CreatePaymentReportRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.CreateAsync(request, cancellationToken));

    /// <summary>Actualiza el monto base y moneda de una relación de pago en borrador.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="request">Monto base y moneda.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}/base-amount")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> UpdateBaseAmount(
        [FromRoute] Guid id,
        [FromBody] UpdatePaymentReportBaseAmountRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.UpdateBaseAmountAsync(id, request, cancellationToken));

    /// <summary>Agrega una partida (adición o deducción) a la relación de pago.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="request">Detalle de la partida.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> AddItem(
        [FromRoute] Guid id,
        [FromBody] AddPaymentItemRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.AddItemAsync(id, request, cancellationToken));

    /// <summary>Actualiza una partida existente.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="itemId">Identificador de la partida.</param>
    /// <param name="request">Datos modificados de la partida.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> UpdateItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        [FromBody] UpdatePaymentItemRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.UpdateItemAsync(id, itemId, request, cancellationToken));

    /// <summary>Elimina una partida de la relación de pago.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="itemId">Identificador de la partida.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> RemoveItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.RemoveItemAsync(id, itemId, cancellationToken));

    /// <summary>Agrega un comentario de auditoría a la relación de pago.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="request">Texto del comentario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> AddComment(
        [FromRoute] Guid id,
        [FromBody] AddPaymentCommentRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.AddCommentAsync(id, request, cancellationToken));

    /// <summary>Adjunta un archivo o comprobante a la relación de pago.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="request">Metadatos del archivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/files")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> AddFile(
        [FromRoute] Guid id,
        [FromBody] AddPaymentReportFileRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.AddFileAsync(id, request, cancellationToken));

    /// <summary>Publica formalmente la relación de pago.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> Publish(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.PublishAsync(id, cancellationToken));

    /// <summary>Anula una relación de pago.</summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="request">Motivo de la anulación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/void")]
    [ProducesResponseType<PaymentReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentReportResponse>> Void(
        [FromRoute] Guid id,
        [FromBody] VoidPaymentReportRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await reportService.VoidAsync(id, request, cancellationToken));
}
