using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies.Events;

public sealed record CompanyProfileUpdatedDomainEvent(
    Guid CompanyId,
    DateTimeOffset OccurredAt) : IDomainEvent;
