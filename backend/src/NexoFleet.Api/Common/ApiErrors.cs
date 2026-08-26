using NexoFleet.Domain.Common;

namespace NexoFleet.Api.Common;

internal static class ApiErrors
{
    public static readonly Error InvalidAntiforgeryToken = Error.Validation(
        "Security.InvalidAntiforgeryToken",
        "El token de seguridad no es válido o ha expirado.");
}
