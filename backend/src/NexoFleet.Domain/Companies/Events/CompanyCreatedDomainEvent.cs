using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies.Events;

public sealed record CompanyCreatedDomainEvent(
    Guid CompanyId,
    DateTimeOffset OccurredAt) : IDomainEvent;
