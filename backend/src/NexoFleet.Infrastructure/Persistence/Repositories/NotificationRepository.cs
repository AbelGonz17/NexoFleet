using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Notifications;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Notifications.SingleOrDefaultAsync(
            notification => notification.CompanyId == companyId && notification.Id == id,
            cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid companyId,
        Guid recipientUserId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Notifications
            .Where(notification =>
                notification.CompanyId == companyId &&
                notification.RecipientUserId == recipientUserId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Notifications
            .Where(notification => notification.CompanyId == companyId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(Notification notification) => dbContext.Notifications.Add(notification);
}
