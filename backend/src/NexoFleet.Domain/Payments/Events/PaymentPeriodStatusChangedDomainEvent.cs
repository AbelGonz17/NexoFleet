using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments.Events;

public sealed record PaymentPeriodStatusChangedDomainEvent(
    Guid PaymentPeriodId,
    Guid CompanyId,
    PaymentPeriodStatus PreviousStatus,
    PaymentPeriodStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
