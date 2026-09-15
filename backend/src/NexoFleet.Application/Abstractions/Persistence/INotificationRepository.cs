using NexoFleet.Domain.Notifications;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(
        Guid? companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid? companyId,
        Guid recipientUserId,
        bool unreadOnly = false,
        string? type = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    void Add(Notification notification);
}
