using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Vehicles;

public static class VehicleErrors
{
    public const int ApprovalReasonMaxLength = 500;
    public const int DocumentFileNameMaxLength = 255;
    public const int DocumentStorageKeyMaxLength = 500;
    public const int DocumentContentTypeMaxLength = 150;

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

    public static readonly Error InvalidVehicleType = Error.Validation(
        "Vehicle.InvalidVehicleType",
        "El tipo de vehículo no es válido.");

    public static readonly Error ApprovalNotRequired = Error.Conflict(
        "Vehicle.ApprovalNotRequired",
        "Los vehículos propiedad de la empresa no requieren aprobación.");

    public static readonly Error AlreadyApproved = Error.Conflict(
        "Vehicle.AlreadyApproved",
        "El vehículo ya está aprobado.");

    public static readonly Error AlreadyRejected = Error.Conflict(
        "Vehicle.AlreadyRejected",
        "El vehículo ya está rechazado.");

    public static readonly Error ApprovalDecisionRequiresPendingStatus = Error.Conflict(
        "Vehicle.ApprovalDecisionRequiresPendingStatus",
        "El vehículo debe estar pendiente para registrar una decisión de aprobación.");

    public static readonly Error ApprovalReasonRequired = Error.Validation(
        "Vehicle.ApprovalReasonRequired",
        "El motivo de la decisión es obligatorio.");

    public static readonly Error ApprovalReasonTooLong = Error.Validation(
        "Vehicle.ApprovalReasonTooLong",
        $"El motivo no puede superar {ApprovalReasonMaxLength} caracteres.");

    public static readonly Error AlreadyOperational = Error.Conflict(
        "Vehicle.AlreadyOperational",
        "El vehículo ya está operativo.");

    public static readonly Error AlreadyInMaintenance = Error.Conflict(
        "Vehicle.AlreadyInMaintenance",
        "El vehículo ya está en mantenimiento.");

    public static readonly Error AlreadyRetired = Error.Conflict(
        "Vehicle.AlreadyRetired",
        "El vehículo ya está retirado.");

    public static readonly Error RetiredStatusIsFinal = Error.Conflict(
        "Vehicle.RetiredStatusIsFinal",
        "Un vehículo retirado no puede cambiar nuevamente de estado.");

    public static readonly Error InvalidDocumentId = Error.Validation("Vehicle.InvalidDocumentId", "El identificador del documento no es válido.");
    public static readonly Error InvalidDocumentType = Error.Validation("Vehicle.InvalidDocumentType", "El tipo de documento no es válido.");
    public static readonly Error DocumentFileNameRequired = Error.Validation("Vehicle.DocumentFileNameRequired", "El nombre del archivo es obligatorio.");
    public static readonly Error DocumentStorageKeyRequired = Error.Validation("Vehicle.DocumentStorageKeyRequired", "La clave de almacenamiento es obligatoria.");
    public static readonly Error DocumentContentTypeRequired = Error.Validation("Vehicle.DocumentContentTypeRequired", "El tipo de contenido es obligatorio.");
    public static readonly Error InvalidDocumentSize = Error.Validation("Vehicle.InvalidDocumentSize", "El tamaño del documento debe ser mayor que cero.");
    public static readonly Error DocumentMetadataTooLong = Error.Validation("Vehicle.DocumentMetadataTooLong", "Los metadatos del documento superan la longitud permitida.");
    public static readonly Error InvalidUploadedByUserId = Error.Validation("Vehicle.InvalidUploadedByUserId", "El usuario que carga el documento no es válido.");
    public static readonly Error DocumentAlreadyExists = Error.Conflict("Vehicle.DocumentAlreadyExists", "El documento ya existe.");
    public static readonly Error DocumentNotFound = Error.NotFound("Vehicle.DocumentNotFound", "El documento no fue encontrado.");
    public static readonly Error NotFound = Error.NotFound("Vehicle.NotFound", "El vehículo no fue encontrado.");
    public static readonly Error LicensePlateDuplicate = Error.Conflict("Vehicle.LicensePlateDuplicate", "Ya existe un vehículo registrado con la misma placa en la empresa.");
}
