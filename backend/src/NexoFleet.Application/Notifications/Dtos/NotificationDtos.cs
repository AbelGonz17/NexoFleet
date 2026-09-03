using NexoFleet.Domain.Notifications;

namespace NexoFleet.Application.Notifications.Dtos;

public sealed record NotificationResponse(
    Guid Id,
    Guid CompanyId,
    Guid RecipientUserId,
    Guid? RecipientEmployeeId,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    string Status,
    DateTimeOffset? ReadAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    DateTimeOffset CreatedAtUtc)
{
    public static NotificationResponse FromDomain(Notification notification) =>
        new(
            notification.Id,
            notification.CompanyId,
            notification.RecipientUserId,
            notification.RecipientEmployeeId,
            notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.RelatedEntityType,
            notification.RelatedEntityId,
            notification.Status.ToString(),
            notification.ReadAtUtc,
            notification.ArchivedAtUtc,
            notification.CreatedAtUtc);
}

public sealed record CreateNotificationRequest(
    Guid RecipientUserId,
    NotificationType Type,
    string Title,
    string Message,
    Guid? RecipientEmployeeId = null,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null);
