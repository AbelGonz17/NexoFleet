using NexoFleet.Domain.Common;
using NexoFleet.Domain.Notifications.Events;

namespace NexoFleet.Domain.Notifications;

public sealed class Notification : AggregateRoot
{
    private Notification(
        Guid id,
        Guid companyId,
        Guid recipientUserId,
        Guid? recipientEmployeeId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        RecipientUserId = recipientUserId;
        RecipientEmployeeId = recipientEmployeeId;
        Type = type;
        Title = title;
        Message = message;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        Status = NotificationStatus.Unread;
        CreatedAtUtc = createdAtUtc;
    }

    private Notification() { }

    public Guid CompanyId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public Guid? RecipientEmployeeId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Result<Notification> Create(
        Guid id,
        Guid companyId,
        Guid recipientUserId,
        Guid? recipientEmployeeId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) return Result<Notification>.Failure(NotificationErrors.InvalidId);
        if (companyId == Guid.Empty) return Result<Notification>.Failure(NotificationErrors.InvalidCompanyId);
        if (recipientUserId == Guid.Empty) return Result<Notification>.Failure(NotificationErrors.InvalidUserId);
        if (recipientEmployeeId == Guid.Empty) return Result<Notification>.Failure(NotificationErrors.InvalidEmployeeId);
        if (!Enum.IsDefined(type)) return Result<Notification>.Failure(NotificationErrors.InvalidType);
        if (string.IsNullOrWhiteSpace(title)) return Result<Notification>.Failure(NotificationErrors.TitleRequired);
        if (title.Trim().Length > NotificationErrors.TitleMaxLength) return Result<Notification>.Failure(NotificationErrors.TitleTooLong);
        if (string.IsNullOrWhiteSpace(message)) return Result<Notification>.Failure(NotificationErrors.MessageRequired);
        if (message.Trim().Length > NotificationErrors.MessageMaxLength) return Result<Notification>.Failure(NotificationErrors.MessageTooLong);
        if (relatedEntityId.HasValue && string.IsNullOrWhiteSpace(relatedEntityType)) return Result<Notification>.Failure(NotificationErrors.RelatedEntityTypeRequired);
        if (!string.IsNullOrWhiteSpace(relatedEntityType) && !relatedEntityId.HasValue) return Result<Notification>.Failure(NotificationErrors.RelatedEntityIdRequired);
        if (relatedEntityType?.Trim().Length > NotificationErrors.RelatedEntityTypeMaxLength) return Result<Notification>.Failure(NotificationErrors.RelatedEntityTypeTooLong);

        var notification = new Notification(id, companyId, recipientUserId, recipientEmployeeId, type,
            title.Trim(), message.Trim(), NormalizeOptional(relatedEntityType), relatedEntityId, createdAtUtc);
        notification.RaiseDomainEvent(new NotificationCreatedDomainEvent(id, companyId, recipientUserId, type, createdAtUtc));
        return Result<Notification>.Success(notification);
    }

    public Result MarkAsRead(DateTimeOffset readAtUtc)
    {
        if (Status == NotificationStatus.Archived) return Result.Failure(NotificationErrors.ArchivedStatusIsFinal);
        if (Status == NotificationStatus.Read) return Result.Failure(NotificationErrors.AlreadyRead);
        Status = NotificationStatus.Read;
        ReadAtUtc = readAtUtc;
        RaiseDomainEvent(new NotificationReadDomainEvent(Id, CompanyId, RecipientUserId, readAtUtc));
        return Result.Success();
    }

    public Result Archive(DateTimeOffset archivedAtUtc)
    {
        if (Status == NotificationStatus.Archived) return Result.Failure(NotificationErrors.AlreadyArchived);
        Status = NotificationStatus.Archived;
        ArchivedAtUtc = archivedAtUtc;
        return Result.Success();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
