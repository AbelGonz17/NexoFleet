using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.RouteSchedules;

public sealed class RouteScheduleAssignment : Entity
{
    internal RouteScheduleAssignment(
        Guid id,
        Guid routeScheduleId,
        Guid companyId,
        Guid employeeId,
        Guid? vehicleId,
        DateOnly validFrom,
        DateOnly? validUntil,
        DateTimeOffset createdAtUtc) : base(id)
    {
        RouteScheduleId = routeScheduleId;
        CompanyId = companyId;
        EmployeeId = employeeId;
        VehicleId = vehicleId;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        CreatedAtUtc = createdAtUtc;
    }

    private RouteScheduleAssignment()
    {
    }

    public Guid RouteScheduleId { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public Guid? VehicleId { get; private set; }

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidUntil { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    internal void Close(DateOnly validUntil, DateTimeOffset updatedAtUtc)
    {
        ValidUntil = validUntil;
        UpdatedAtUtc = updatedAtUtc;
    }
}
