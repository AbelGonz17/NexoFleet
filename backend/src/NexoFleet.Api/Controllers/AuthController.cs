using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Dtos.Authentication;
using NexoFleet.Api.Extensions;
using NexoFleet.Api.Filters;
using NexoFleet.Api.Services;
using NexoFleet.Application.Authentication;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    AuthService authService,
    AntiforgeryTokenService antiforgeryTokenService) : ControllerBase
{
    /// <summary>Genera el token CSRF necesario para las operaciones de autenticación.</summary>
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ProducesResponseType<CsrfTokenResponse>(StatusCodes.Status200OK)]
    public ActionResult<CsrfTokenResponse> Csrf() =>
        Ok(antiforgeryTokenService.IssueToken(HttpContext));

    /// <summary>Inicia una sesión usando correo y contraseña.</summary>
    /// <param name="request">Credenciales del usuario.</param>
    /// <param name="antiforgeryToken">Token obtenido desde GET /api/v1/auth/csrf.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    [AllowAnonymous]
    [RequireAntiforgeryToken]
    [HttpPost("login")]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthenticatedUser>> Login(
        [FromBody] LoginRequest request,
        [FromHeader(Name = "X-XSRF-TOKEN")] string? antiforgeryToken,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await authService.LoginAsync(request, cancellationToken));

    /// <summary>Devuelve el usuario asociado con la sesión activa.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUser>> Me(CancellationToken cancellationToken) =>
        this.ToActionResult(await authService.GetCurrentUserAsync(cancellationToken));

    /// <summary>Cierra la sesión activa.</summary>
    /// <param name="antiforgeryToken">Token obtenido desde GET /api/v1/auth/csrf.</param>
    [Authorize]
    [RequireAntiforgeryToken]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "X-XSRF-TOKEN")] string? antiforgeryToken) =>
        this.ToNoContentResult(await authService.LogoutAsync());
}
