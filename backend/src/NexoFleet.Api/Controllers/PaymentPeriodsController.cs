using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Payments;
using NexoFleet.Application.Payments.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/payment-periods")]
public sealed class PaymentPeriodsController(PaymentPeriodService periodService) : ControllerBase
{
    /// <summary>Lista los periodos de pago de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PaymentPeriodResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentPeriodResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await periodService.ListAsync(cancellationToken));

    /// <summary>Obtiene un periodo de pago por su identificador.</summary>
    /// <param name="id">Identificador del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentPeriodResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentPeriodResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await periodService.GetByIdAsync(id, cancellationToken));

    /// <summary>Crea un nuevo periodo de pago.</summary>
    /// <param name="request">Rango de fechas y código del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<PaymentPeriodResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentPeriodResponse>> Create(
        [FromBody] CreatePaymentPeriodRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await periodService.CreateAsync(request, cancellationToken));

    /// <summary>Cierra un periodo de pago abierto.</summary>
    /// <param name="id">Identificador del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await periodService.CloseAsync(id, cancellationToken));

    /// <summary>Reabre un periodo de pago cerrado.</summary>
    /// <param name="id">Identificador del periodo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reopen(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await periodService.ReopenAsync(id, cancellationToken));
}
