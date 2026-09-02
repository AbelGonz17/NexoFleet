using FluentValidation;
using NexoFleet.Application.Vehicles.Dtos;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Validators;

public sealed class AddVehicleDocumentRequestValidator : AbstractValidator<AddVehicleDocumentRequest>
{
    public AddVehicleDocumentRequestValidator()
    {
        RuleFor(request => request.Type)
            .IsInEnum().WithMessage(VehicleErrors.InvalidDocumentType.Description);

        RuleFor(request => request.FileName)
            .NotEmpty().WithMessage(VehicleErrors.DocumentFileNameRequired.Description)
            .MaximumLength(VehicleErrors.DocumentFileNameMaxLength).WithMessage(VehicleErrors.DocumentMetadataTooLong.Description);

        RuleFor(request => request.StorageKey)
            .NotEmpty().WithMessage(VehicleErrors.DocumentStorageKeyRequired.Description)
            .MaximumLength(VehicleErrors.DocumentStorageKeyMaxLength).WithMessage(VehicleErrors.DocumentMetadataTooLong.Description);

        RuleFor(request => request.ContentType)
            .NotEmpty().WithMessage(VehicleErrors.DocumentContentTypeRequired.Description)
            .MaximumLength(VehicleErrors.DocumentContentTypeMaxLength).WithMessage(VehicleErrors.DocumentMetadataTooLong.Description);

        RuleFor(request => request.SizeInBytes)
            .GreaterThan(0).WithMessage(VehicleErrors.InvalidDocumentSize.Description);
    }
}
