using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Notifications;

public static class NotificationErrors
{
    public const int TitleMaxLength = 200;
    public const int MessageMaxLength = 2000;
    public const int RelatedEntityTypeMaxLength = 100;

    public static readonly Error InvalidId = Error.Validation("Notification.InvalidId", "The notification identifier is invalid.");
    public static readonly Error InvalidCompanyId = Error.Validation("Notification.InvalidCompanyId", "The company identifier is invalid.");
    public static readonly Error InvalidUserId = Error.Validation("Notification.InvalidUserId", "The recipient user identifier is invalid.");
    public static readonly Error InvalidEmployeeId = Error.Validation("Notification.InvalidEmployeeId", "The recipient employee identifier is invalid.");
    public static readonly Error InvalidType = Error.Validation("Notification.InvalidType", "The notification type is invalid.");
    public static readonly Error TitleRequired = Error.Validation("Notification.TitleRequired", "The title is required.");
    public static readonly Error TitleTooLong = Error.Validation("Notification.TitleTooLong", "The title is too long.");
    public static readonly Error MessageRequired = Error.Validation("Notification.MessageRequired", "The message is required.");
    public static readonly Error MessageTooLong = Error.Validation("Notification.MessageTooLong", "The message is too long.");
    public static readonly Error RelatedEntityTypeRequired = Error.Validation("Notification.RelatedEntityTypeRequired", "The related entity type is required when an identifier is provided.");
    public static readonly Error RelatedEntityIdRequired = Error.Validation("Notification.RelatedEntityIdRequired", "The related entity identifier is required when a type is provided.");
    public static readonly Error RelatedEntityTypeTooLong = Error.Validation("Notification.RelatedEntityTypeTooLong", "The related entity type is too long.");
    public static readonly Error AlreadyRead = Error.Conflict("Notification.AlreadyRead", "The notification is already read.");
    public static readonly Error AlreadyArchived = Error.Conflict("Notification.AlreadyArchived", "The notification is already archived.");
    public static readonly Error ArchivedStatusIsFinal = Error.Conflict("Notification.ArchivedStatusIsFinal", "An archived notification cannot be changed.");
}
