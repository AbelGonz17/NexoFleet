using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using NexoFleet.Application.Authentication;

namespace NexoFleet.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    AuthService authService,
    IAntiforgery antiforgery) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ProducesResponseType<CsrfTokenResponse>(StatusCodes.Status200OK)]
    public ActionResult<CsrfTokenResponse> Csrf()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new CsrfTokenResponse(tokens.RequestToken ?? string.Empty));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthenticatedUser>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!await HasValidAntiforgeryTokenAsync())
        {
            return InvalidAntiforgeryToken();
        }

        LoginResult result;

        try
        {
            result = await authService.LoginAsync(request, cancellationToken);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        return result.Status switch
        {
            LoginStatus.Success => Ok(result.User),
            LoginStatus.LockedOut => Problem(
                statusCode: StatusCodes.Status423Locked,
                title: "Cuenta bloqueada temporalmente"),
            LoginStatus.Inactive => UnauthorizedProblem("La cuenta está inactiva."),
            _ => UnauthorizedProblem("El correo o la contraseña no son válidos.")
        };
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUser>> Me(CancellationToken cancellationToken)
    {
        var user = await authService.GetCurrentUserAsync(cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        if (!await HasValidAntiforgeryTokenAsync())
        {
            return InvalidAntiforgeryToken();
        }

        await authService.LogoutAsync();
        return NoContent();
    }

    private async Task<bool> HasValidAntiforgeryTokenAsync()
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }

    private ObjectResult InvalidAntiforgeryToken() => Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Solicitud no válida",
        detail: "El token de seguridad no es válido o ha expirado.");

    private ObjectResult UnauthorizedProblem(string detail) => Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "No fue posible iniciar sesión",
        detail: detail);
}

public sealed record CsrfTokenResponse(string Token);
