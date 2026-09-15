using System.Text.Json;
using MediatR;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Auditing;
using NexoFleet.Domain.Clients.Events;
using NexoFleet.Domain.Companies.Events;
using NexoFleet.Domain.Employees.Events;
using NexoFleet.Domain.Payments.Events;
using NexoFleet.Domain.Routes.Events;
using NexoFleet.Domain.RouteSchedules.Events;
using NexoFleet.Domain.Trips.Events;
using NexoFleet.Domain.Vehicles.Events;

namespace NexoFleet.Application.Auditing.EventHandlers;

internal sealed class AuditLogDomainEventHandlers(
    IAuditLogRepository auditLogRepository,
    ICurrentUser currentUser) :
    INotificationHandler<CompanyCreatedDomainEvent>,
    INotificationHandler<CompanyStatusChangedDomainEvent>,
    INotificationHandler<CompanyProfileUpdatedDomainEvent>,
    INotificationHandler<TripCreatedDomainEvent>,
    INotificationHandler<TripAssignedDomainEvent>,
    INotificationHandler<TripStatusChangedDomainEvent>,
    INotificationHandler<TripCompletedDomainEvent>,
    INotificationHandler<VehicleCreatedDomainEvent>,
    INotificationHandler<VehicleStatusChangedDomainEvent>,
    INotificationHandler<VehicleApprovalStatusChangedDomainEvent>,
    INotificationHandler<EmployeeCreatedDomainEvent>,
    INotificationHandler<EmployeeStatusChangedDomainEvent>,
    INotificationHandler<ClientCreatedDomainEvent>,
    INotificationHandler<ClientStatusChangedDomainEvent>,
    INotificationHandler<PaymentPeriodCreatedDomainEvent>,
    INotificationHandler<PaymentPeriodStatusChangedDomainEvent>,
    INotificationHandler<PaymentReportPublishedDomainEvent>
{
    private void Log(Guid? companyId, Guid? entityId, string entityType, string action, object? data, DateTimeOffset occurredAtUtc)
    {
        var auditLog = AuditLog.Create(
            id: Guid.NewGuid(),
            companyId: companyId,
            actorUserId: currentUser.UserId ?? Guid.Empty,
            action: action,
            entityType: entityType,
            entityId: entityId,
            data: data != null ? JsonSerializer.Serialize(data) : null,
            ipAddress: null,
            userAgent: "System",
            severity: AuditLogSeverity.Info,
            actorEmail: currentUser.Email ?? "system@nexofleet.test",
            actorRole: currentUser.Role ?? "System",
            occurredAtUtc: occurredAtUtc
        );

        if (auditLog.IsSuccess)
        {
            auditLogRepository.Add(auditLog.Value);
        }
    }

    public Task Handle(CompanyCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.CompanyId, "Company", "COMPANY_CREATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(CompanyStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.CompanyId, "Company", "COMPANY_STATUS_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(CompanyProfileUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.CompanyId, "Company", "COMPANY_PROFILE_UPDATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(TripCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.TripId, "Trip", "TRIP_CREATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(TripAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.TripId, "Trip", "TRIP_ASSIGNED", new { notification.EmployeeId, notification.VehicleId }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(TripStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.TripId, "Trip", "TRIP_STATUS_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(TripCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.TripId, "Trip", "TRIP_COMPLETED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(VehicleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.VehicleId, "Vehicle", "VEHICLE_CREATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(VehicleStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.VehicleId, "Vehicle", "VEHICLE_STATUS_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(VehicleApprovalStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.VehicleId, "Vehicle", "VEHICLE_APPROVAL_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(EmployeeCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.EmployeeId, "Employee", "EMPLOYEE_CREATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(EmployeeStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.EmployeeId, "Employee", "EMPLOYEE_STATUS_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(ClientCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.ClientId, "Client", "CLIENT_CREATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(ClientStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.ClientId, "Client", "CLIENT_STATUS_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(PaymentPeriodCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.PaymentPeriodId, "PaymentPeriod", "PAYMENT_PERIOD_CREATED", null, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(PaymentPeriodStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.PaymentPeriodId, "PaymentPeriod", "PAYMENT_PERIOD_STATUS_CHANGED", new { notification.PreviousStatus, notification.CurrentStatus }, notification.OccurredAt);
        return Task.CompletedTask;
    }

    public Task Handle(PaymentReportPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        Log(notification.CompanyId, notification.PaymentReportId, "PaymentReport", "PAYMENT_REPORT_PUBLISHED", new { notification.TotalAmount, notification.Currency }, notification.OccurredAt);
        return Task.CompletedTask;
    }
}
