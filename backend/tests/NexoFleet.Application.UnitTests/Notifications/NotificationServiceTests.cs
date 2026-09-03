using NexoFleet.Application.Notifications;
using NexoFleet.Application.Notifications.Dtos;
using NexoFleet.Application.Notifications.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Notifications;

namespace NexoFleet.Application.UnitTests.Notifications;

public sealed class NotificationServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task NotificationLifecycleShouldCreateReadAndArchive()
    {
        var repo = new FakeNotificationRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var user = new FakeCurrentUser(UserId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = new NotificationService(repo, tenant, user, uow, clock, new CreateNotificationRequestValidator());

        var request = new CreateNotificationRequest(
            UserId,
            NotificationType.TripAssigned,
            "Nuevo viaje asignado",
            "Se le ha asignado el viaje TRIP-001");

        var createResult = await service.CreateAsync(request);
        Assert.True(createResult.IsSuccess);
        Assert.Equal(NotificationStatus.Unread.ToString(), createResult.Value.Status);

        var notifId = createResult.Value.Id;

        // Mark as read
        var readResult = await service.MarkAsReadAsync(notifId);
        Assert.True(readResult.IsSuccess);

        var readNotif = repo.Notifications.Single(n => n.Id == notifId);
        Assert.Equal(NotificationStatus.Read, readNotif.Status);

        // Archive
        var archiveResult = await service.ArchiveAsync(notifId);
        Assert.True(archiveResult.IsSuccess);
        Assert.Equal(NotificationStatus.Archived, readNotif.Status);
    }
}
