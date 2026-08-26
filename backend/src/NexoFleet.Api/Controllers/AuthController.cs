using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Api.Common;
using NexoFleet.Api.Extensions;
using NexoFleet.Application.Authentication;
using NexoFleet.Domain.Common;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    AuthService authService,
    IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Genera el token CSRF necesario para las operaciones de autenticación.</summary>
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ProducesResponseType<CsrfTokenResponse>(StatusCodes.Status200OK)]
    public ActionResult<CsrfTokenResponse> Csrf()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new CsrfTokenResponse(tokens.RequestToken ?? string.Empty));
    }

    /// <summary>Inicia una sesión usando correo y contraseña.</summary>
    /// <param name="request">Credenciales del usuario.</param>
    /// <param name="antiforgeryToken">Token obtenido desde GET /api/v1/auth/csrf.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthenticatedUser>> Login(
        [FromBody] LoginRequest request,
        [FromHeader(Name = "X-XSRF-TOKEN")] string? antiforgeryToken,
        CancellationToken cancellationToken)
    {
        _ = antiforgeryToken;
        var antiforgeryResult = await ValidateAntiforgeryTokenAsync();
        if (antiforgeryResult.IsFailure)
        {
            return this.ToActionResult<AuthenticatedUser>(
                Result<AuthenticatedUser>.Failure(antiforgeryResult.Error));
        }

        var result = await authService.LoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Devuelve el usuario asociado con la sesión activa.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUser>> Me(CancellationToken cancellationToken)
    {
        var user = await authService.GetCurrentUserAsync(cancellationToken);
        return this.ToActionResult(user);
    }

    /// <summary>Cierra la sesión activa.</summary>
    /// <param name="antiforgeryToken">Token obtenido desde GET /api/v1/auth/csrf.</param>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "X-XSRF-TOKEN")] string? antiforgeryToken)
    {
        _ = antiforgeryToken;
        var antiforgeryResult = await ValidateAntiforgeryTokenAsync();
        if (antiforgeryResult.IsFailure)
        {
            return this.ToNoContentResult(antiforgeryResult);
        }

        var result = await authService.LogoutAsync();
        return this.ToNoContentResult(result);
    }

    private async Task<Result> ValidateAntiforgeryTokenAsync()
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
            return Result.Success();
        }
        catch (AntiforgeryValidationException)
        {
            return Result.Failure(ApiErrors.InvalidAntiforgeryToken);
        }
    }
}

public sealed record CsrfTokenResponse(string Token);
