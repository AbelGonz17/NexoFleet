using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class ApproveTripRequestValidator : AbstractValidator<ApproveTripRequest>
{
    public ApproveTripRequestValidator()
    {
        When(request => !string.IsNullOrWhiteSpace(request.Comments), () =>
        {
            RuleFor(request => request.Comments!)
                .MaximumLength(TripErrors.NotesMaxLength).WithMessage(TripErrors.NotesTooLong.Description);
        });
    }
}
