using MediatR;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Authentication;
using NexoFleet.Domain.Companies.Events;
using NexoFleet.Domain.Notifications;
using NexoFleet.Domain.Payments.Events;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Trips.Events;
using NexoFleet.Domain.Vehicles.Events;

namespace NexoFleet.Application.Notifications.EventHandlers;

internal sealed class NotificationDomainEventHandlers(
    INotificationRepository notificationRepository,
    IIdentityService identityService) :
    INotificationHandler<CompanyCreatedDomainEvent>,
    INotificationHandler<TripStatusChangedDomainEvent>,
    INotificationHandler<VehicleApprovalStatusChangedDomainEvent>,
    INotificationHandler<PaymentReportPublishedDomainEvent>
{
    private async Task NotifyUsers(IEnumerable<AuthenticatedUser> users, Guid companyId, NotificationType type, string title, string message, string? relatedEntityType, Guid? relatedEntityId, DateTimeOffset createdAtUtc)
    {
        foreach (var user in users)
        {
            var notif = Notification.Create(
                id: Guid.NewGuid(),
                companyId: companyId,
                recipientUserId: user.Id,
                recipientEmployeeId: null,
                type: type,
                title: title,
                message: message,
                relatedEntityType: relatedEntityType,
                relatedEntityId: relatedEntityId,
                createdAtUtc: createdAtUtc
            );

            if (notif.IsSuccess)
            {
                notificationRepository.Add(notif.Value);
            }
        }
    }

    public async Task Handle(CompanyCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var superAdmins = await identityService.GetUsersByRoleAsync("SuperAdmin", cancellationToken);
        await NotifyUsers(superAdmins, notification.CompanyId, NotificationType.System, "Nueva Empresa Registrada", "Una nueva empresa se ha registrado en el sistema y podría requerir validación.", "Company", notification.CompanyId, notification.OccurredAt);
    }

    public async Task Handle(TripStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.CurrentStatus == TripStatus.Cancelled)
        {
            var admins = await identityService.GetUsersByCompanyIdAsync(notification.CompanyId, cancellationToken);
            // Filtrar solo admins (asumiendo que en la lista viene el Rol o simplemente enviamos a todos los de la empresa que sean admins)
            var companyAdmins = admins.Where(u => u.Roles.Contains("Admin")).ToList();

            await NotifyUsers(companyAdmins, notification.CompanyId, NotificationType.TripStatusChanged, "Viaje Cancelado", "Un viaje ha sido cancelado.", "Trip", notification.TripId, notification.OccurredAt);
        }
    }

    public async Task Handle(VehicleApprovalStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var admins = await identityService.GetUsersByCompanyIdAsync(notification.CompanyId, cancellationToken);
        var companyAdmins = admins.Where(u => u.Roles.Contains("Admin")).ToList();

        await NotifyUsers(companyAdmins, notification.CompanyId, NotificationType.VehicleReview, "Estado de Aprobación de Vehículo Actualizado", $"El vehículo ha cambiado su estado de aprobación a {notification.CurrentStatus}.", "Vehicle", notification.VehicleId, notification.OccurredAt);
    }

    public async Task Handle(PaymentReportPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        var admins = await identityService.GetUsersByCompanyIdAsync(notification.CompanyId, cancellationToken);
        var companyAdmins = admins.Where(u => u.Roles.Contains("Admin")).ToList();

        await NotifyUsers(companyAdmins, notification.CompanyId, NotificationType.PaymentReportPublished, "Reporte de Pagos Publicado", "Se ha publicado un nuevo reporte de pagos.", "PaymentReport", notification.PaymentReportId, notification.OccurredAt);
    }
}
