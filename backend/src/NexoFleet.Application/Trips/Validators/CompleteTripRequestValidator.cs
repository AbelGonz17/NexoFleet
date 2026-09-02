using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class CompleteTripRequestValidator : AbstractValidator<CompleteTripRequest>
{
    public CompleteTripRequestValidator()
    {
        RuleFor(request => request.FinalAmount)
            .GreaterThanOrEqualTo(0).WithMessage(TripErrors.InvalidAmount.Description);

        RuleFor(request => request.Currency)
            .NotEmpty().WithMessage(TripErrors.CurrencyRequired.Description)
            .Length(TripErrors.CurrencyLength).WithMessage(TripErrors.CurrencyInvalid.Description);
    }
}
