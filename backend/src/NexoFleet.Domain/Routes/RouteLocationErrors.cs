using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes;

public static class RouteLocationErrors
{
    public static readonly Error AddressRequired = Error.Validation(
        "RouteLocation.AddressRequired",
        "La dirección de la ubicación es obligatoria.");

    public static readonly Error AddressTooLong = Error.Validation(
        "RouteLocation.AddressTooLong",
        $"La dirección no puede superar {RouteLocation.AddressMaxLength} caracteres.");

    public static readonly Error CoordinatesIncomplete = Error.Validation(
        "RouteLocation.CoordinatesIncomplete",
        "La latitud y la longitud deben indicarse juntas.");

    public static readonly Error LatitudeOutOfRange = Error.Validation(
        "RouteLocation.LatitudeOutOfRange",
        "La latitud debe estar entre -90 y 90.");

    public static readonly Error LongitudeOutOfRange = Error.Validation(
        "RouteLocation.LongitudeOutOfRange",
        "La longitud debe estar entre -180 y 180.");
}
