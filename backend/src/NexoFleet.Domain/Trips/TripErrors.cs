using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips;

public static class TripErrors
{
    public const int CurrencyLength = 3;
    public const int TripNumberMaxLength = 50;
    public const int NotesMaxLength = 1000;
    public const int IncidentDescriptionMaxLength = 2000;
    public const int FileNameMaxLength = 255;
    public const int StorageKeyMaxLength = 500;
    public const int ContentTypeMaxLength = 150;

    public static readonly Error InvalidId = Error.Validation("Trip.InvalidId", "The trip identifier is invalid.");
    public static readonly Error InvalidCompanyId = Error.Validation("Trip.InvalidCompanyId", "The company identifier is invalid.");
    public static readonly Error InvalidClientId = Error.Validation("Trip.InvalidClientId", "The client identifier is invalid.");
    public static readonly Error InvalidRouteId = Error.Validation("Trip.InvalidRouteId", "The route identifier is invalid.");
    public static readonly Error InvalidRouteScheduleId = Error.Validation("Trip.InvalidRouteScheduleId", "The route schedule identifier is invalid.");
    public static readonly Error InvalidEmployeeId = Error.Validation("Trip.InvalidEmployeeId", "The employee identifier is invalid.");
    public static readonly Error InvalidVehicleId = Error.Validation("Trip.InvalidVehicleId", "The vehicle identifier is invalid.");
    public static readonly Error InvalidUserId = Error.Validation("Trip.InvalidUserId", "The user identifier is invalid.");
    public static readonly Error TripNumberRequired = Error.Validation("Trip.TripNumberRequired", "The trip number is required.");
    public static readonly Error TripNumberTooLong = Error.Validation("Trip.TripNumberTooLong", "The trip number is too long.");
    public static readonly Error OriginRequired = Error.Validation("Trip.OriginRequired", "The trip origin is required.");
    public static readonly Error DestinationRequired = Error.Validation("Trip.DestinationRequired", "The trip destination is required.");
    public static readonly Error InvalidAmount = Error.Validation("Trip.InvalidAmount", "The trip amount cannot be negative.");
    public static readonly Error AmountRequired = Error.Validation("Trip.AmountRequired", "An amount is required when a currency is provided.");
    public static readonly Error CurrencyRequired = Error.Validation("Trip.CurrencyRequired", "A currency is required when an amount is provided.");
    public static readonly Error CurrencyInvalid = Error.Validation("Trip.CurrencyInvalid", "The currency must contain three letters.");
    public static readonly Error PendingApprovalRequired = Error.Conflict("Trip.PendingApprovalRequired", "Only a pending trip can be reviewed.");
    public static readonly Error ReviewReasonRequired = Error.Validation("Trip.ReviewReasonRequired", "A reason is required to reject a trip.");
    public static readonly Error NotesTooLong = Error.Validation("Trip.NotesTooLong", "The notes are too long.");
    public static readonly Error AssignmentNotAllowed = Error.Conflict("Trip.AssignmentNotAllowed", "The trip cannot be assigned in its current status.");
    public static readonly Error AssignmentAlreadyExists = Error.Conflict("Trip.AssignmentAlreadyExists", "The assignment already exists.");
    public static readonly Error AssignmentCannotRewriteHistory = Error.Conflict("Trip.AssignmentCannotRewriteHistory", "A reassignment cannot occur before the current assignment.");
    public static readonly Error CurrentAssignmentRequired = Error.Conflict("Trip.CurrentAssignmentRequired", "The trip requires a current assignment.");
    public static readonly Error AssignedEmployeeMismatch = Error.Forbidden("Trip.AssignedEmployeeMismatch", "The employee is not assigned to this trip.");
    public static readonly Error StartNotAllowed = Error.Conflict("Trip.StartNotAllowed", "Only an assigned trip can be started.");
    public static readonly Error InvalidStartTime = Error.Validation("Trip.InvalidStartTime", "The trip cannot start before its current assignment.");
    public static readonly Error CompletionNotAllowed = Error.Conflict("Trip.CompletionNotAllowed", "Only a trip in progress can be completed.");
    public static readonly Error InvalidCompletionTime = Error.Validation("Trip.InvalidCompletionTime", "The completion time must be after the start time.");
    public static readonly Error CancellationNotAllowed = Error.Conflict("Trip.CancellationNotAllowed", "The trip cannot be cancelled in its current status.");
    public static readonly Error CancellationReasonRequired = Error.Validation("Trip.CancellationReasonRequired", "A cancellation reason is required.");
    public static readonly Error InvalidChildId = Error.Validation("Trip.InvalidChildId", "The child entity identifier is invalid.");
    public static readonly Error IncidentNotAllowed = Error.Conflict("Trip.IncidentNotAllowed", "An incident cannot be registered in the current status.");
    public static readonly Error IncidentDescriptionRequired = Error.Validation("Trip.IncidentDescriptionRequired", "The incident description is required.");
    public static readonly Error IncidentDescriptionTooLong = Error.Validation("Trip.IncidentDescriptionTooLong", "The incident description is too long.");
    public static readonly Error InvalidIncidentSeverity = Error.Validation("Trip.InvalidIncidentSeverity", "The incident severity is invalid.");
    public static readonly Error FileNameRequired = Error.Validation("Trip.FileNameRequired", "The file name is required.");
    public static readonly Error StorageKeyRequired = Error.Validation("Trip.StorageKeyRequired", "The storage key is required.");
    public static readonly Error ContentTypeRequired = Error.Validation("Trip.ContentTypeRequired", "The content type is required.");
    public static readonly Error InvalidFileSize = Error.Validation("Trip.InvalidFileSize", "The file size must be greater than zero.");
    public static readonly Error FileMetadataTooLong = Error.Validation("Trip.FileMetadataTooLong", "The file metadata is too long.");
    public static readonly Error ChildAlreadyExists = Error.Conflict("Trip.ChildAlreadyExists", "The child entity already exists.");
    public static readonly Error RouteRequiredForSchedule = Error.Validation("Trip.RouteRequiredForSchedule", "A scheduled trip requires a route.");
}
