using NexoFleet.Domain.Notifications;
using NexoFleet.Domain.Notifications.Events;

namespace NexoFleet.Domain.UnitTests.Notifications;

public sealed class NotificationTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldStartUnreadAndRaiseEvent()
    {
        var result = CreateNotification();

        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationStatus.Unread, result.Value.Status);
        Assert.IsType<NotificationCreatedDomainEvent>(result.Value.DomainEvents.Single());
    }

    [Fact]
    public void RelatedEntityDataMustBeComplete()
    {
        var result = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            NotificationType.General, "Title", "Message", "Trip", null, Now);

        Assert.Equal(NotificationErrors.RelatedEntityIdRequired, result.Error);
    }

    [Fact]
    public void MarkAsReadShouldRecordTimestampAndRaiseEvent()
    {
        var notification = CreateNotification().Value;
        notification.ClearDomainEvents();

        var result = notification.MarkAsRead(Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationStatus.Read, notification.Status);
        Assert.Equal(Now.AddMinutes(1), notification.ReadAtUtc);
        Assert.IsType<NotificationReadDomainEvent>(notification.DomainEvents.Single());
    }

    [Fact]
    public void ArchivedNotificationCannotBeRead()
    {
        var notification = CreateNotification().Value;
        notification.Archive(Now.AddMinutes(1));

        var result = notification.MarkAsRead(Now.AddMinutes(2));

        Assert.Equal(NotificationErrors.ArchivedStatusIsFinal, result.Error);
    }

    private static NexoFleet.Domain.Common.Result<Notification> CreateNotification() => Notification.Create(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), NotificationType.TripAssigned,
        " New trip ", " A trip was assigned to you. ", "Trip", Guid.NewGuid(), Now);
}
