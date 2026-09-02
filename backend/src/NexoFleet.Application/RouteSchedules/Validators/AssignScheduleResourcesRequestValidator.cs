using FluentValidation;
using NexoFleet.Application.RouteSchedules.Dtos;
using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.RouteSchedules.Validators;

public sealed class AssignScheduleResourcesRequestValidator : AbstractValidator<AssignScheduleResourcesRequest>
{
    public AssignScheduleResourcesRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .NotEmpty().WithMessage(RouteScheduleErrors.InvalidEmployeeId.Description);

        When(request => request.VehicleId.HasValue, () =>
        {
            RuleFor(request => request.VehicleId!.Value)
                .NotEmpty().WithMessage(RouteScheduleErrors.InvalidVehicleId.Description);
        });

        When(request => request.ValidUntil.HasValue, () =>
        {
            RuleFor(request => request.ValidUntil!.Value)
                .GreaterThanOrEqualTo(request => request.ValidFrom).WithMessage(RouteScheduleErrors.InvalidAssignmentPeriod.Description);
        });
    }
}
