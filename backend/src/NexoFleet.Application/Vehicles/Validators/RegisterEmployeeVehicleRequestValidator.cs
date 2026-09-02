using FluentValidation;
using NexoFleet.Application.Vehicles.Dtos;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Validators;

public sealed class RegisterEmployeeVehicleRequestValidator : AbstractValidator<RegisterEmployeeVehicleRequest>
{
    public RegisterEmployeeVehicleRequestValidator()
    {
        RuleFor(request => request.OwnerEmployeeId)
            .NotEmpty().WithMessage(VehicleErrors.InvalidOwnerEmployeeId.Description);

        RuleFor(request => request.LicensePlate)
            .NotEmpty().WithMessage(VehicleErrors.LicensePlateRequired.Description)
            .MaximumLength(Vehicle.LicensePlateMaxLength).WithMessage(VehicleErrors.LicensePlateTooLong.Description);

        RuleFor(request => request.Make)
            .NotEmpty().WithMessage(VehicleErrors.MakeRequired.Description)
            .MaximumLength(Vehicle.MakeMaxLength).WithMessage(VehicleErrors.MakeTooLong.Description);

        RuleFor(request => request.Model)
            .NotEmpty().WithMessage(VehicleErrors.ModelRequired.Description)
            .MaximumLength(Vehicle.ModelMaxLength).WithMessage(VehicleErrors.ModelTooLong.Description);

        RuleFor(request => request.ManufactureYear)
            .GreaterThanOrEqualTo(Vehicle.MinimumManufactureYear).WithMessage(VehicleErrors.InvalidManufactureYear.Description);

        When(request => !string.IsNullOrWhiteSpace(request.Color), () =>
        {
            RuleFor(request => request.Color!)
                .MaximumLength(Vehicle.ColorMaxLength).WithMessage(VehicleErrors.ColorTooLong.Description);
        });

        RuleFor(request => request.Type)
            .IsInEnum().WithMessage(VehicleErrors.InvalidVehicleType.Description);

        When(request => request.PassengerCapacity.HasValue, () =>
        {
            RuleFor(request => request.PassengerCapacity!.Value)
                .GreaterThan(0).WithMessage(VehicleErrors.InvalidPassengerCapacity.Description);
        });
    }
}
