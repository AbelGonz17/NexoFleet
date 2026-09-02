using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class AddTripIncidentRequestValidator : AbstractValidator<AddTripIncidentRequest>
{
    public AddTripIncidentRequestValidator()
    {
        RuleFor(request => request.ReportedByEmployeeId)
            .NotEmpty().WithMessage(TripErrors.InvalidEmployeeId.Description);

        RuleFor(request => request.Severity)
            .IsInEnum().WithMessage(TripErrors.InvalidIncidentSeverity.Description);

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage(TripErrors.IncidentDescriptionRequired.Description)
            .MaximumLength(TripErrors.IncidentDescriptionMaxLength).WithMessage(TripErrors.IncidentDescriptionTooLong.Description);
    }
}
