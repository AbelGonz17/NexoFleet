using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Notifications.Events;

public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    Guid CompanyId,
    Guid RecipientUserId,
    NotificationType Type,
    DateTimeOffset OccurredAt) : IDomainEvent;
