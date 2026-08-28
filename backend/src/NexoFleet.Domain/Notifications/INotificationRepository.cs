namespace NexoFleet.Domain.Notifications;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByRecipientAsync(Guid recipientUserId, CancellationToken cancellationToken = default);
    void Add(Notification notification);
}
