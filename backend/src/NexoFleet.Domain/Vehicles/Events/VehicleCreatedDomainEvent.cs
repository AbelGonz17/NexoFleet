using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Vehicles.Events;

public sealed record VehicleCreatedDomainEvent(
    Guid VehicleId,
    Guid CompanyId,
    VehicleOwnershipType OwnershipType,
    DateTimeOffset OccurredAt) : IDomainEvent;
