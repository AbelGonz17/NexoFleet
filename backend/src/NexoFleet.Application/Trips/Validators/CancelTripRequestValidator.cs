using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class CancelTripRequestValidator : AbstractValidator<CancelTripRequest>
{
    public CancelTripRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty().WithMessage(TripErrors.CancellationReasonRequired.Description)
            .MaximumLength(TripErrors.NotesMaxLength).WithMessage(TripErrors.NotesTooLong.Description);
    }
}
