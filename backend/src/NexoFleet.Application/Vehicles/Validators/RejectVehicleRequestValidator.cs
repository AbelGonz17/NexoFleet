using FluentValidation;
using NexoFleet.Application.Vehicles.Dtos;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Validators;

public sealed class RejectVehicleRequestValidator : AbstractValidator<RejectVehicleRequest>
{
    public RejectVehicleRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty().WithMessage(VehicleErrors.ApprovalReasonRequired.Description)
            .MaximumLength(VehicleErrors.ApprovalReasonMaxLength).WithMessage(VehicleErrors.ApprovalReasonTooLong.Description);
    }
}
