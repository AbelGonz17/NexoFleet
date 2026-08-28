using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments;

public static class PaymentErrors
{
    public const int CodeMaxLength = 50;
    public const int DescriptionMaxLength = 500;
    public const int CommentMaxLength = 2000;
    public const int ReasonMaxLength = 1000;
    public const int CurrencyLength = 3;
    public const int FileNameMaxLength = 255;
    public const int StorageKeyMaxLength = 500;
    public const int ContentTypeMaxLength = 150;

    public static readonly Error InvalidId = Error.Validation("Payment.InvalidId", "The identifier is invalid.");
    public static readonly Error InvalidCompanyId = Error.Validation("Payment.InvalidCompanyId", "The company identifier is invalid.");
    public static readonly Error InvalidPeriodId = Error.Validation("Payment.InvalidPeriodId", "The payment period identifier is invalid.");
    public static readonly Error InvalidEmployeeId = Error.Validation("Payment.InvalidEmployeeId", "The employee identifier is invalid.");
    public static readonly Error InvalidTripId = Error.Validation("Payment.InvalidTripId", "The trip identifier is invalid.");
    public static readonly Error InvalidUserId = Error.Validation("Payment.InvalidUserId", "The user identifier is invalid.");
    public static readonly Error CodeRequired = Error.Validation("Payment.CodeRequired", "The period code is required.");
    public static readonly Error CodeTooLong = Error.Validation("Payment.CodeTooLong", "The period code is too long.");
    public static readonly Error InvalidPeriod = Error.Validation("Payment.InvalidPeriod", "The period end date must be on or after its start date.");
    public static readonly Error AlreadyOpen = Error.Conflict("Payment.AlreadyOpen", "The period is already open.");
    public static readonly Error AlreadyClosed = Error.Conflict("Payment.AlreadyClosed", "The period is already closed.");
    public static readonly Error InvalidAmount = Error.Validation("Payment.InvalidAmount", "The amount cannot be negative.");
    public static readonly Error CurrencyRequired = Error.Validation("Payment.CurrencyRequired", "The currency is required.");
    public static readonly Error CurrencyInvalid = Error.Validation("Payment.CurrencyInvalid", "The currency must contain three letters.");
    public static readonly Error DraftRequired = Error.Conflict("Payment.DraftRequired", "Only a draft report can be modified.");
    public static readonly Error ItemAlreadyExists = Error.Conflict("Payment.ItemAlreadyExists", "The payment item already exists.");
    public static readonly Error ItemNotFound = Error.NotFound("Payment.ItemNotFound", "The payment item was not found.");
    public static readonly Error InvalidEffect = Error.Validation("Payment.InvalidEffect", "The payment item effect is invalid.");
    public static readonly Error DescriptionRequired = Error.Validation("Payment.DescriptionRequired", "The description is required.");
    public static readonly Error DescriptionTooLong = Error.Validation("Payment.DescriptionTooLong", "The description is too long.");
    public static readonly Error CommentRequired = Error.Validation("Payment.CommentRequired", "The comment is required.");
    public static readonly Error CommentTooLong = Error.Validation("Payment.CommentTooLong", "The comment is too long.");
    public static readonly Error FileRequiredToPublish = Error.Conflict("Payment.FileRequiredToPublish", "A payment report file is required before publishing.");
    public static readonly Error AlreadyPublished = Error.Conflict("Payment.AlreadyPublished", "The report is already published.");
    public static readonly Error VoidedStatusIsFinal = Error.Conflict("Payment.VoidedStatusIsFinal", "A voided report cannot be changed.");
    public static readonly Error VoidReasonRequired = Error.Validation("Payment.VoidReasonRequired", "A reason is required to void the report.");
    public static readonly Error ReasonTooLong = Error.Validation("Payment.ReasonTooLong", "The reason is too long.");
    public static readonly Error FileNameRequired = Error.Validation("Payment.FileNameRequired", "The file name is required.");
    public static readonly Error StorageKeyRequired = Error.Validation("Payment.StorageKeyRequired", "The storage key is required.");
    public static readonly Error ContentTypeRequired = Error.Validation("Payment.ContentTypeRequired", "The content type is required.");
    public static readonly Error InvalidFileSize = Error.Validation("Payment.InvalidFileSize", "The file size must be greater than zero.");
    public static readonly Error FileMetadataTooLong = Error.Validation("Payment.FileMetadataTooLong", "The file metadata is too long.");
    public static readonly Error CommentAlreadyExists = Error.Conflict("Payment.CommentAlreadyExists", "The comment already exists.");
    public static readonly Error FileAlreadyExists = Error.Conflict("Payment.FileAlreadyExists", "The payment report file already exists.");
}
