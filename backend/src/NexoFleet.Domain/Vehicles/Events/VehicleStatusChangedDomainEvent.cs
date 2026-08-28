using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Vehicles.Events;

public sealed record VehicleStatusChangedDomainEvent(
    Guid VehicleId,
    Guid CompanyId,
    VehicleStatus PreviousStatus,
    VehicleStatus CurrentStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
