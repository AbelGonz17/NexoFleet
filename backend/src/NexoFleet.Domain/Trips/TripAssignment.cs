using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips;

public sealed class TripAssignment : Entity
{
    internal TripAssignment(
        Guid id,
        Guid tripId,
        Guid companyId,
        Guid employeeId,
        Guid? vehicleId,
        Guid assignedByUserId,
        DateTimeOffset assignedAtUtc) : base(id)
    {
        TripId = tripId;
        CompanyId = companyId;
        EmployeeId = employeeId;
        VehicleId = vehicleId;
        AssignedByUserId = assignedByUserId;
        AssignedAtUtc = assignedAtUtc;
    }

    private TripAssignment() { }

    public Guid TripId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid? VehicleId { get; private set; }
    public Guid AssignedByUserId { get; private set; }
    public DateTimeOffset AssignedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }

    internal void End(DateTimeOffset endedAtUtc) => EndedAtUtc = endedAtUtc;
}
