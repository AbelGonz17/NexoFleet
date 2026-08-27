using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies.Events;

public sealed record CompanyStatusChangedDomainEvent(
    Guid CompanyId,
    CompanyStatus PreviousStatus,
    CompanyStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
