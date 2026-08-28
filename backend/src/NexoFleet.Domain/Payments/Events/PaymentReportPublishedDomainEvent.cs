using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments.Events;

public sealed record PaymentReportPublishedDomainEvent(
    Guid PaymentReportId,
    Guid CompanyId,
    Guid EmployeeId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAt) : IDomainEvent;
