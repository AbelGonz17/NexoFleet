using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Companies;
using NexoFleet.Application.Companies.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/companies")]
public sealed class CompaniesController(CompanyService companyService) : ControllerBase
{
    /// <summary>Lista todas las empresas registradas en el sistema.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CompanyResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await companyService.ListAsync(cancellationToken));

    /// <summary>Obtiene el detalle de una empresa por su identificador.</summary>
    /// <param name="id">Identificador único de la empresa.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await companyService.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra una nueva empresa en el sistema.</summary>
    /// <param name="request">Datos de la empresa a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyResponse>> Create(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await companyService.CreateAsync(request, cancellationToken));

    /// <summary>Actualiza el perfil de una empresa.</summary>
    /// <param name="id">Identificador único de la empresa.</param>
    /// <param name="request">Datos actualizados del perfil.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> UpdateProfile(
        [FromRoute] Guid id,
        [FromBody] UpdateCompanyProfileRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await companyService.UpdateProfileAsync(id, request, cancellationToken));

    /// <summary>Suspende temporalmente las operaciones de una empresa.</summary>
    /// <param name="id">Identificador de la empresa.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suspend(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await companyService.SuspendAsync(id, cancellationToken));

    /// <summary>Activa una empresa previamente suspendida.</summary>
    /// <param name="id">Identificador de la empresa.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await companyService.ActivateAsync(id, cancellationToken));
}
