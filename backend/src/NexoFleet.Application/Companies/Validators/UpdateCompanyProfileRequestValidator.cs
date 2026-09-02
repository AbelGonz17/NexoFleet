using FluentValidation;
using NexoFleet.Application.Companies.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.Companies.Validators;

public sealed class UpdateCompanyProfileRequestValidator : AbstractValidator<UpdateCompanyProfileRequest>
{
    public UpdateCompanyProfileRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage(CompanyErrors.NameRequired.Description)
            .MaximumLength(CompanyName.MaxLength).WithMessage(CompanyErrors.NameTooLong.Description);

        RuleFor(request => request.TaxIdentification)
            .NotEmpty().WithMessage(CompanyErrors.TaxIdentificationRequired.Description)
            .MaximumLength(TaxIdentification.MaxLength).WithMessage(CompanyErrors.TaxIdentificationTooLong.Description);

        RuleFor(request => request.Country)
            .NotEmpty().WithMessage(CompanyErrors.CountryRequired.Description)
            .MaximumLength(Address.CountryMaxLength).WithMessage(CompanyErrors.CountryTooLong.Description);

        RuleFor(request => request.City)
            .NotEmpty().WithMessage(CompanyErrors.CityRequired.Description)
            .MaximumLength(Address.CityMaxLength).WithMessage(CompanyErrors.CityTooLong.Description);

        RuleFor(request => request.Phone)
            .NotEmpty().WithMessage(CompanyErrors.PhoneRequired.Description)
            .MaximumLength(PhoneNumber.MaxLength).WithMessage(CompanyErrors.PhoneTooLong.Description);

        RuleFor(request => request.Email)
            .NotEmpty().WithMessage(CompanyErrors.EmailRequired.Description)
            .EmailAddress().WithMessage(CompanyErrors.EmailInvalid.Description)
            .MaximumLength(Email.MaxLength).WithMessage(CompanyErrors.EmailTooLong.Description);
    }
}
