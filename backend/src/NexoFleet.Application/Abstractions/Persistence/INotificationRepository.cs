using NexoFleet.Domain.Notifications;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid companyId,
        Guid recipientUserId,
        CancellationToken cancellationToken = default);

    void Add(Notification notification);
}
