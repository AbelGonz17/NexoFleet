using FluentValidation;
using NexoFleet.Application.Clients.Dtos;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.Clients.Validators;

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(request => request.ClientCode)
            .NotEmpty().WithMessage(ClientErrors.ClientCodeRequired.Description)
            .MaximumLength(ClientCode.MaxLength).WithMessage(ClientErrors.ClientCodeTooLong.Description);

        RuleFor(request => request.Name)
            .NotEmpty().WithMessage(ClientErrors.NameRequired.Description)
            .MaximumLength(ClientName.MaxLength).WithMessage(ClientErrors.NameTooLong.Description);

        When(request => !string.IsNullOrWhiteSpace(request.TaxIdentification), () =>
        {
            RuleFor(request => request.TaxIdentification!)
                .MaximumLength(TaxIdentification.MaxLength).WithMessage(ClientErrors.TaxIdentificationTooLong.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.ContactName), () =>
        {
            RuleFor(request => request.ContactName!)
                .MaximumLength(ContactName.MaxLength).WithMessage(ClientErrors.ContactNameTooLong.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.Phone), () =>
        {
            RuleFor(request => request.Phone!)
                .MaximumLength(PhoneNumber.MaxLength).WithMessage(ClientErrors.PhoneTooLong.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.Email), () =>
        {
            RuleFor(request => request.Email!)
                .EmailAddress().WithMessage(ClientErrors.EmailInvalid.Description)
                .MaximumLength(Email.MaxLength).WithMessage(ClientErrors.EmailTooLong.Description);
        });
    }
}
