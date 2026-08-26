using Microsoft.AspNetCore.Mvc;
using NexoFleet.Domain.Common;

namespace NexoFleet.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult<TValue> ToActionResult<TValue>(
        this ControllerBase controller,
        Result<TValue> result)
    {
        return result.IsSuccess
            ? controller.Ok(result.Value)
            : controller.HttpContext.ToErrorResult(result.Error);
    }

    public static IActionResult ToNoContentResult(
        this ControllerBase controller,
        Result result)
    {
        return result.IsSuccess
            ? controller.NoContent()
            : controller.HttpContext.ToErrorResult(result.Error);
    }

    public static ObjectResult ToErrorResult(this HttpContext httpContext, Error error)
    {
        if (error is ValidationError validationError)
        {
            var validationProblem = new ValidationProblemDetails(
                validationError.Errors.ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = validationError.Description,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            };

            AddExtensions(httpContext, validationProblem, validationError.Code);
            return new BadRequestObjectResult(validationProblem);
        }

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Locked => StatusCodes.Status423Locked,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(error.Type),
            Detail = error.Description
        };

        AddExtensions(httpContext, problem, error.Code);
        return new ObjectResult(problem) { StatusCode = statusCode };
    }

    private static void AddExtensions(
        HttpContext httpContext,
        ProblemDetails problem,
        string errorCode)
    {
        problem.Extensions["code"] = errorCode;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
    }

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Solicitud no válida",
        ErrorType.Unauthorized => "No autorizado",
        ErrorType.Forbidden => "Acceso denegado",
        ErrorType.NotFound => "Recurso no encontrado",
        ErrorType.Conflict => "Conflicto",
        ErrorType.Locked => "Recurso bloqueado",
        _ => "Error interno"
    };
}
