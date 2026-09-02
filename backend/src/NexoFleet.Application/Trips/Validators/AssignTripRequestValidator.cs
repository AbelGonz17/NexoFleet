using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class AssignTripRequestValidator : AbstractValidator<AssignTripRequest>
{
    public AssignTripRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .NotEmpty().WithMessage(TripErrors.InvalidEmployeeId.Description);

        When(request => request.VehicleId.HasValue, () =>
        {
            RuleFor(request => request.VehicleId!.Value)
                .NotEmpty().WithMessage(TripErrors.InvalidVehicleId.Description);
        });
    }
}
