using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Vehicles.Events;

public sealed record VehicleApprovalStatusChangedDomainEvent(
    Guid VehicleId,
    Guid CompanyId,
    VehicleApprovalStatus PreviousStatus,
    VehicleApprovalStatus CurrentStatus,
    string? Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
