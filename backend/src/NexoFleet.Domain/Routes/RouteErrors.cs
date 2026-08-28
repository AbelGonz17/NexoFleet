using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes;

public static class RouteErrors
{
    public static readonly Error InvalidId = Error.Validation(
        "Route.InvalidId",
        "El identificador de la ruta no es válido.");

    public static readonly Error InvalidCompanyId = Error.Validation(
        "Route.InvalidCompanyId",
        "El identificador de la empresa no es válido.");

    public static readonly Error InvalidClientId = Error.Validation(
        "Route.InvalidClientId",
        "El identificador del cliente no es válido.");

    public static readonly Error RouteCodeRequired = Error.Validation(
        "Route.RouteCodeRequired",
        "El código de la ruta es obligatorio.");

    public static readonly Error RouteCodeTooLong = Error.Validation(
        "Route.RouteCodeTooLong",
        $"El código de la ruta no puede superar {Route.RouteCodeMaxLength} caracteres.");

    public static readonly Error NameRequired = Error.Validation(
        "Route.NameRequired",
        "El nombre de la ruta es obligatorio.");

    public static readonly Error NameTooLong = Error.Validation(
        "Route.NameTooLong",
        $"El nombre de la ruta no puede superar {Route.NameMaxLength} caracteres.");

    public static readonly Error OriginRequired = Error.Validation(
        "Route.OriginRequired",
        "El origen de la ruta es obligatorio.");

    public static readonly Error OriginTooLong = Error.Validation(
        "Route.OriginTooLong",
        $"El origen no puede superar {Route.OriginMaxLength} caracteres.");

    public static readonly Error DestinationRequired = Error.Validation(
        "Route.DestinationRequired",
        "El destino de la ruta es obligatorio.");

    public static readonly Error DestinationTooLong = Error.Validation(
        "Route.DestinationTooLong",
        $"El destino no puede superar {Route.DestinationMaxLength} caracteres.");

    public static readonly Error InstructionsTooLong = Error.Validation(
        "Route.InstructionsTooLong",
        $"Las instrucciones no pueden superar {Route.InstructionsMaxLength} caracteres.");

    public static readonly Error InvalidEstimatedDuration = Error.Validation(
        "Route.InvalidEstimatedDuration",
        "La duración estimada debe ser mayor que cero.");

    public static readonly Error InvalidReferenceAmount = Error.Validation(
        "Route.InvalidReferenceAmount",
        "La tarifa de referencia no puede ser negativa.");

    public static readonly Error ReferenceAmountRequired = Error.Validation(
        "Route.ReferenceAmountRequired",
        "Debe indicar una tarifa cuando especifica una moneda.");

    public static readonly Error ReferenceCurrencyRequired = Error.Validation(
        "Route.ReferenceCurrencyRequired",
        "Debe indicar la moneda de la tarifa de referencia.");

    public static readonly Error ReferenceCurrencyInvalid = Error.Validation(
        "Route.ReferenceCurrencyInvalid",
        "La moneda debe utilizar un código de tres letras.");

    public static readonly Error InvalidStopId = Error.Validation(
        "Route.InvalidStopId",
        "El identificador de la parada no es válido.");

    public static readonly Error StopAlreadyExists = Error.Conflict(
        "Route.StopAlreadyExists",
        "Ya existe una parada con ese identificador en la ruta.");

    public static readonly Error StopNotFound = Error.NotFound(
        "Route.StopNotFound",
        "La parada indicada no existe en la ruta.");

    public static readonly Error StopAddressRequired = Error.Validation(
        "Route.StopAddressRequired",
        "La dirección de la parada es obligatoria.");

    public static readonly Error StopAddressTooLong = Error.Validation(
        "Route.StopAddressTooLong",
        $"La dirección de la parada no puede superar {RouteStop.AddressMaxLength} caracteres.");

    public static readonly Error StopInstructionsTooLong = Error.Validation(
        "Route.StopInstructionsTooLong",
        $"Las instrucciones de la parada no pueden superar {RouteStop.InstructionsMaxLength} caracteres.");

    public static readonly Error InvalidStopSequence = Error.Validation(
        "Route.InvalidStopSequence",
        "La posición de la parada no es válida.");

    public static readonly Error AlreadyActive = Error.Conflict(
        "Route.AlreadyActive",
        "La ruta ya está activa.");

    public static readonly Error AlreadyInactive = Error.Conflict(
        "Route.AlreadyInactive",
        "La ruta ya está inactiva.");
}
