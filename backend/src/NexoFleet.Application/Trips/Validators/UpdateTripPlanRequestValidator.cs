using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class UpdateTripPlanRequestValidator : AbstractValidator<UpdateTripPlanRequest>
{
    public UpdateTripPlanRequestValidator()
    {
        RuleFor(request => request.Origin)
            .NotNull().WithMessage(TripErrors.OriginRequired.Description);

        When(request => request.Origin is not null, () =>
        {
            RuleFor(request => request.Origin.Address)
                .NotEmpty().WithMessage(RouteLocationErrors.AddressRequired.Description)
                .MaximumLength(RouteLocation.AddressMaxLength).WithMessage(RouteLocationErrors.AddressTooLong.Description);

            When(request => request.Origin.Latitude.HasValue || request.Origin.Longitude.HasValue, () =>
            {
                RuleFor(request => request.Origin.Latitude)
                    .NotNull().WithMessage(RouteLocationErrors.CoordinatesIncomplete.Description)
                    .InclusiveBetween(-90, 90).WithMessage(RouteLocationErrors.LatitudeOutOfRange.Description);

                RuleFor(request => request.Origin.Longitude)
                    .NotNull().WithMessage(RouteLocationErrors.CoordinatesIncomplete.Description)
                    .InclusiveBetween(-180, 180).WithMessage(RouteLocationErrors.LongitudeOutOfRange.Description);
            });
        });

        RuleFor(request => request.Destination)
            .NotNull().WithMessage(TripErrors.DestinationRequired.Description);

        When(request => request.Destination is not null, () =>
        {
            RuleFor(request => request.Destination.Address)
                .NotEmpty().WithMessage(RouteLocationErrors.AddressRequired.Description)
                .MaximumLength(RouteLocation.AddressMaxLength).WithMessage(RouteLocationErrors.AddressTooLong.Description);

            When(request => request.Destination.Latitude.HasValue || request.Destination.Longitude.HasValue, () =>
            {
                RuleFor(request => request.Destination.Latitude)
                    .NotNull().WithMessage(RouteLocationErrors.CoordinatesIncomplete.Description)
                    .InclusiveBetween(-90, 90).WithMessage(RouteLocationErrors.LatitudeOutOfRange.Description);

                RuleFor(request => request.Destination.Longitude)
                    .NotNull().WithMessage(RouteLocationErrors.CoordinatesIncomplete.Description)
                    .InclusiveBetween(-180, 180).WithMessage(RouteLocationErrors.LongitudeOutOfRange.Description);
            });
        });

        When(request => request.AgreedAmount.HasValue, () =>
        {
            RuleFor(request => request.AgreedAmount!.Value)
                .GreaterThanOrEqualTo(0).WithMessage(TripErrors.InvalidAmount.Description);

            RuleFor(request => request.Currency)
                .NotEmpty().WithMessage(TripErrors.CurrencyRequired.Description)
                .Length(TripErrors.CurrencyLength).WithMessage(TripErrors.CurrencyInvalid.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.Currency), () =>
        {
            RuleFor(request => request.AgreedAmount)
                .NotNull().WithMessage(TripErrors.AmountRequired.Description);
        });
    }
}
