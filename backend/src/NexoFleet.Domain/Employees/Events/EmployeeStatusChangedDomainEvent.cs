using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Employees.Events;

public sealed record EmployeeStatusChangedDomainEvent(
    Guid EmployeeId,
    Guid CompanyId,
    EmployeeStatus PreviousStatus,
    EmployeeStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
