using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments.Events;

public sealed record PaymentPeriodCreatedDomainEvent(Guid PaymentPeriodId, Guid CompanyId, DateTimeOffset OccurredAt) : IDomainEvent;
