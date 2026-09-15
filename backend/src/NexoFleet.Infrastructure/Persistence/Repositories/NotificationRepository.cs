using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Notifications;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(
        Guid? companyId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Notifications.Where(n => n.Id == id);
        if (companyId.HasValue) query = query.Where(n => n.CompanyId == companyId.Value);
        return query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid? companyId,
        Guid recipientUserId,
        bool unreadOnly = false,
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Notifications
            .Where(notification => notification.RecipientUserId == recipientUserId);

        if (companyId.HasValue)
        {
            query = query.Where(n => n.CompanyId == companyId.Value);
        }

        if (unreadOnly)
        {
            query = query.Where(n => n.Status == NotificationStatus.Unread);
        }

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, true, out var parsedType))
        {
            query = query.Where(n => n.Type == parsedType);
        }

        return await query
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Notifications
            .Where(notification => notification.CompanyId == companyId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(Notification notification) => dbContext.Notifications.Add(notification);
}
