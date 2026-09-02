using FluentValidation;
using NexoFleet.Application.Employees.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;

namespace NexoFleet.Application.Employees.Validators;

public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(request => request.EmployeeCode)
            .NotEmpty().WithMessage(EmployeeErrors.EmployeeCodeRequired.Description)
            .MaximumLength(EmployeeCode.MaxLength).WithMessage(EmployeeErrors.EmployeeCodeTooLong.Description);

        RuleFor(request => request.FirstName)
            .NotEmpty().WithMessage(EmployeeErrors.FirstNameRequired.Description)
            .MaximumLength(FullName.FirstNameMaxLength).WithMessage(EmployeeErrors.FirstNameTooLong.Description);

        RuleFor(request => request.LastName)
            .NotEmpty().WithMessage(EmployeeErrors.LastNameRequired.Description)
            .MaximumLength(FullName.LastNameMaxLength).WithMessage(EmployeeErrors.LastNameTooLong.Description);

        RuleFor(request => request.IdentityDocument)
            .NotEmpty().WithMessage(EmployeeErrors.IdentityDocumentRequired.Description)
            .MaximumLength(IdentityDocument.MaxLength).WithMessage(EmployeeErrors.IdentityDocumentTooLong.Description);

        RuleFor(request => request.Phone)
            .NotEmpty().WithMessage(EmployeeErrors.PhoneRequired.Description)
            .MaximumLength(PhoneNumber.MaxLength).WithMessage(EmployeeErrors.PhoneTooLong.Description);

        RuleFor(request => request.Email)
            .NotEmpty().WithMessage(EmployeeErrors.EmailInvalid.Description)
            .EmailAddress().WithMessage(EmployeeErrors.EmailInvalid.Description)
            .MaximumLength(Email.MaxLength).WithMessage(EmployeeErrors.EmailTooLong.Description);
    }
}
