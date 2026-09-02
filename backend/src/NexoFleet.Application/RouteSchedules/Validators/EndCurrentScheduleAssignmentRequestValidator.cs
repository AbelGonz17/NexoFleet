using FluentValidation;
using NexoFleet.Application.RouteSchedules.Dtos;
using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.RouteSchedules.Validators;

public sealed class EndCurrentScheduleAssignmentRequestValidator : AbstractValidator<EndCurrentScheduleAssignmentRequest>
{
    public EndCurrentScheduleAssignmentRequestValidator()
    {
        RuleFor(request => request.ValidUntil)
            .NotEmpty().WithMessage(RouteScheduleErrors.InvalidAssignmentPeriod.Description);
    }
}
