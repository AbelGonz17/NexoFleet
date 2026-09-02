using FluentValidation;
using NexoFleet.Application.Routes.Dtos;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Routes.Validators;

public sealed class UpdateRouteDetailsRequestValidator : AbstractValidator<UpdateRouteDetailsRequest>
{
    public UpdateRouteDetailsRequestValidator()
    {
        RuleFor(request => request.RouteCode)
            .NotEmpty().WithMessage(RouteErrors.RouteCodeRequired.Description)
            .MaximumLength(Route.RouteCodeMaxLength).WithMessage(RouteErrors.RouteCodeTooLong.Description);

        RuleFor(request => request.Name)
            .NotEmpty().WithMessage(RouteErrors.NameRequired.Description)
            .MaximumLength(Route.NameMaxLength).WithMessage(RouteErrors.NameTooLong.Description);

        RuleFor(request => request.Origin)
            .NotNull().WithMessage(RouteErrors.OriginRequired.Description);

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
            .NotNull().WithMessage(RouteErrors.DestinationRequired.Description);

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

        When(request => !string.IsNullOrWhiteSpace(request.Instructions), () =>
        {
            RuleFor(request => request.Instructions!)
                .MaximumLength(Route.InstructionsMaxLength).WithMessage(RouteErrors.InstructionsTooLong.Description);
        });

        When(request => request.EstimatedDurationMinutes.HasValue, () =>
        {
            RuleFor(request => request.EstimatedDurationMinutes!.Value)
                .GreaterThan(0).WithMessage(RouteErrors.InvalidEstimatedDuration.Description);
        });

        When(request => request.ReferenceAmount.HasValue, () =>
        {
            RuleFor(request => request.ReferenceAmount!.Value)
                .GreaterThanOrEqualTo(0).WithMessage(RouteErrors.InvalidReferenceAmount.Description);

            RuleFor(request => request.ReferenceCurrency)
                .NotEmpty().WithMessage(RouteErrors.ReferenceCurrencyRequired.Description)
                .Length(Route.CurrencyLength).WithMessage(RouteErrors.ReferenceCurrencyInvalid.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.ReferenceCurrency), () =>
        {
            RuleFor(request => request.ReferenceAmount)
                .NotNull().WithMessage(RouteErrors.ReferenceAmountRequired.Description);
        });
    }
}
