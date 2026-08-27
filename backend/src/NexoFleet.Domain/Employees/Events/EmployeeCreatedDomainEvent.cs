using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Employees.Events;

public sealed record EmployeeCreatedDomainEvent(
    Guid EmployeeId,
    Guid CompanyId,
    DateTimeOffset OccurredAt) : IDomainEvent;
