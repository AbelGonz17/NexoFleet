using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Vehicles;

public static class VehicleErrors
{
    public static readonly Error InvalidId = Error.Validation(
        "Vehicle.InvalidId",
        "El identificador del vehículo no es válido.");

    public static readonly Error InvalidCompanyId = Error.Validation(
        "Vehicle.InvalidCompanyId",
        "El identificador de la empresa no es válido.");

    public static readonly Error InvalidOwnerEmployeeId = Error.Validation(
        "Vehicle.InvalidOwnerEmployeeId",
        "El identificador del empleado propietario no es válido.");

    public static readonly Error LicensePlateRequired = Error.Validation(
        "Vehicle.LicensePlateRequired",
        "La placa del vehículo es obligatoria.");

    public static readonly Error LicensePlateTooLong = Error.Validation(
        "Vehicle.LicensePlateTooLong",
        $"La placa no puede superar {Vehicle.LicensePlateMaxLength} caracteres.");

    public static readonly Error MakeRequired = Error.Validation(
        "Vehicle.MakeRequired",
        "La marca del vehículo es obligatoria.");

    public static readonly Error MakeTooLong = Error.Validation(
        "Vehicle.MakeTooLong",
        $"La marca no puede superar {Vehicle.MakeMaxLength} caracteres.");

    public static readonly Error ModelRequired = Error.Validation(
        "Vehicle.ModelRequired",
        "El modelo del vehículo es obligatorio.");

    public static readonly Error ModelTooLong = Error.Validation(
        "Vehicle.ModelTooLong",
        $"El modelo no puede superar {Vehicle.ModelMaxLength} caracteres.");

    public static readonly Error ColorTooLong = Error.Validation(
        "Vehicle.ColorTooLong",
        $"El color no puede superar {Vehicle.ColorMaxLength} caracteres.");

    public static readonly Error InvalidManufactureYear = Error.Validation(
        "Vehicle.InvalidManufactureYear",
        $"El año del vehículo debe ser igual o posterior a {Vehicle.MinimumManufactureYear} y no puede superar el próximo año.");

    public static readonly Error InvalidPassengerCapacity = Error.Validation(
        "Vehicle.InvalidPassengerCapacity",
        "La capacidad de pasajeros debe ser mayor que cero.");

    public static readonly Error AlreadyAvailable = Error.Conflict(
        "Vehicle.AlreadyAvailable",
        "El vehículo ya está disponible.");

    public static readonly Error AlreadyInService = Error.Conflict(
        "Vehicle.AlreadyInService",
        "El vehículo ya está en servicio.");

    public static readonly Error AlreadyInMaintenance = Error.Conflict(
        "Vehicle.AlreadyInMaintenance",
        "El vehículo ya está en mantenimiento.");

    public static readonly Error AlreadyRetired = Error.Conflict(
        "Vehicle.AlreadyRetired",
        "El vehículo ya está retirado.");

    public static readonly Error RetiredStatusIsFinal = Error.Conflict(
        "Vehicle.RetiredStatusIsFinal",
        "Un vehículo retirado no puede cambiar nuevamente de estado.");

    public static readonly Error NotInService = Error.Conflict(
        "Vehicle.NotInService",
        "El vehículo no está actualmente en servicio.");

    public static readonly Error MaintenanceVehicleCannotStartService = Error.Conflict(
        "Vehicle.MaintenanceVehicleCannotStartService",
        "Un vehículo en mantenimiento no puede iniciar un servicio.");

    public static readonly Error InServiceVehicleCannotEnterMaintenance = Error.Conflict(
        "Vehicle.InServiceVehicleCannotEnterMaintenance",
        "Un vehículo en servicio debe finalizar su servicio antes de entrar en mantenimiento.");

    public static readonly Error InServiceVehicleCannotBeRetired = Error.Conflict(
        "Vehicle.InServiceVehicleCannotBeRetired",
        "Un vehículo en servicio debe finalizar su servicio antes de ser retirado.");

    public static readonly Error InServiceVehicleCannotBeMarkedAvailable = Error.Conflict(
        "Vehicle.InServiceVehicleCannotBeMarkedAvailable",
        "El servicio del vehículo debe finalizarse mediante la operación correspondiente.");
}
