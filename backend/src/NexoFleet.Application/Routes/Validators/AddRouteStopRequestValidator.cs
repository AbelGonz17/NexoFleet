using FluentValidation;
using NexoFleet.Application.Routes.Dtos;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Routes.Validators;

public sealed class AddRouteStopRequestValidator : AbstractValidator<AddRouteStopRequest>
{
    public AddRouteStopRequestValidator()
    {
        RuleFor(request => request.Location)
            .NotNull().WithMessage(RouteErrors.StopLocationRequired.Description);

        When(request => request.Location is not null, () =>
        {
            RuleFor(request => request.Location.Address)
                .NotEmpty().WithMessage(RouteLocationErrors.AddressRequired.Description)
                .MaximumLength(RouteLocation.AddressMaxLength).WithMessage(RouteLocationErrors.AddressTooLong.Description);

            When(request => request.Location.Latitude.HasValue || request.Location.Longitude.HasValue, () =>
            {
                RuleFor(request => request.Location.Latitude)
                    .NotNull().WithMessage(RouteLocationErrors.CoordinatesIncomplete.Description)
                    .InclusiveBetween(-90, 90).WithMessage(RouteLocationErrors.LatitudeOutOfRange.Description);

                RuleFor(request => request.Location.Longitude)
                    .NotNull().WithMessage(RouteLocationErrors.CoordinatesIncomplete.Description)
                    .InclusiveBetween(-180, 180).WithMessage(RouteLocationErrors.LongitudeOutOfRange.Description);
            });
        });

        When(request => !string.IsNullOrWhiteSpace(request.Instructions), () =>
        {
            RuleFor(request => request.Instructions!)
                .MaximumLength(RouteStop.InstructionsMaxLength).WithMessage(RouteErrors.StopInstructionsTooLong.Description);
        });
    }
}
