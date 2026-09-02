using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class RejectTripRequestValidator : AbstractValidator<RejectTripRequest>
{
    public RejectTripRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty().WithMessage(TripErrors.ReviewReasonRequired.Description)
            .MaximumLength(TripErrors.NotesMaxLength).WithMessage(TripErrors.NotesTooLong.Description);
    }
}
