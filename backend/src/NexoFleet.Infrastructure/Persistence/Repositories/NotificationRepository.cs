using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Notifications;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Notifications.SingleOrDefaultAsync(notification => notification.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(Guid recipientUserId, CancellationToken cancellationToken = default) =>
        await dbContext.Notifications
            .Where(notification => notification.RecipientUserId == recipientUserId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(Notification notification) => dbContext.Notifications.Add(notification);
}
