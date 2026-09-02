using FluentValidation;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Validators;

public sealed class AddTripFileRequestValidator : AbstractValidator<AddTripFileRequest>
{
    public AddTripFileRequestValidator()
    {
        RuleFor(request => request.FileName)
            .NotEmpty().WithMessage(TripErrors.FileNameRequired.Description)
            .MaximumLength(TripErrors.FileNameMaxLength).WithMessage(TripErrors.FileMetadataTooLong.Description);

        RuleFor(request => request.StorageKey)
            .NotEmpty().WithMessage(TripErrors.StorageKeyRequired.Description)
            .MaximumLength(TripErrors.StorageKeyMaxLength).WithMessage(TripErrors.FileMetadataTooLong.Description);

        RuleFor(request => request.ContentType)
            .NotEmpty().WithMessage(TripErrors.ContentTypeRequired.Description)
            .MaximumLength(TripErrors.ContentTypeMaxLength).WithMessage(TripErrors.FileMetadataTooLong.Description);

        RuleFor(request => request.SizeInBytes)
            .GreaterThan(0).WithMessage(TripErrors.InvalidFileSize.Description);
    }
}
