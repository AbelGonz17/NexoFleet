using NexoFleet.Domain.Common;

namespace NexoFleet.Application.Authentication;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Auth.InvalidCredentials",
        "El correo o la contraseña no son válidos.");

    public static readonly Error Inactive = Error.Unauthorized(
        "Auth.Inactive",
        "La cuenta está inactiva.");

    public static readonly Error LockedOut = Error.Locked(
        "Auth.LockedOut",
        "La cuenta está bloqueada temporalmente.");

    public static readonly Error SessionNotFound = Error.Unauthorized(
        "Auth.SessionNotFound",
        "No existe una sesión activa.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Auth.EmailAlreadyExists",
        "El correo electrónico ya se encuentra registrado en el sistema.");

    public static readonly Error CompanyInactive = Error.Forbidden(
        "Auth.CompanyInactive",
        "La empresa asociada a esta cuenta se encuentra suspendida o deshabilitada.");
}
