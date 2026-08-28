using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Notifications.Events;

public sealed record NotificationReadDomainEvent(
    Guid NotificationId,
    Guid CompanyId,
    Guid RecipientUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
