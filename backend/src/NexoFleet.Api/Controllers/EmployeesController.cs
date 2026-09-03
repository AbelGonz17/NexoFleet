using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Employees;
using NexoFleet.Application.Employees.Dtos;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/employees")]
public sealed class EmployeesController(EmployeeService employeeService) : ControllerBase
{
    /// <summary>Lista los empleados de la empresa actual.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await employeeService.ListAsync(cancellationToken));

    /// <summary>Obtiene un empleado por su identificador.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await employeeService.GetByIdAsync(id, cancellationToken));

    /// <summary>Registra un nuevo empleado o conductor.</summary>
    /// <param name="request">Datos del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await employeeService.CreateAsync(request, cancellationToken));

    /// <summary>Actualiza el perfil de un empleado.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="request">Datos a modificar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await employeeService.UpdateProfileAsync(id, request, cancellationToken));

    /// <summary>Vincula una cuenta de usuario al empleado.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="request">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/link-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkUser(
        [FromRoute] Guid id,
        [FromBody] LinkUserAccountRequest request,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await employeeService.LinkUserAccountAsync(id, request, cancellationToken));

    /// <summary>Desvincula la cuenta de usuario del empleado.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/unlink-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await employeeService.UnlinkUserAccountAsync(id, cancellationToken));

    /// <summary>Suspende temporalmente a un empleado.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suspend(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await employeeService.SuspendAsync(id, cancellationToken));

    /// <summary>Reactiva a un empleado suspendido.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await employeeService.ActivateAsync(id, cancellationToken));

    /// <summary>Retira definitivamente a un empleado.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{id:guid}/retire")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Retire(
        [FromRoute] Guid id,
        CancellationToken cancellationToken) =>
        this.ToNoContentResult(await employeeService.RetireAsync(id, cancellationToken));
}
