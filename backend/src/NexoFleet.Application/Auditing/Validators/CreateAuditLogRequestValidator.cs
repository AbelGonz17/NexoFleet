using FluentValidation;
using NexoFleet.Application.Auditing.Dtos;
using NexoFleet.Domain.Auditing;

namespace NexoFleet.Application.Auditing.Validators;

public sealed class CreateAuditLogRequestValidator : AbstractValidator<CreateAuditLogRequest>
{
    public CreateAuditLogRequestValidator()
    {
        RuleFor(request => request.Action)
            .NotEmpty().WithMessage(AuditLogErrors.ActionRequired.Description)
            .MaximumLength(AuditLogErrors.ActionMaxLength).WithMessage(AuditLogErrors.ActionTooLong.Description);

        RuleFor(request => request.EntityType)
            .NotEmpty().WithMessage(AuditLogErrors.EntityTypeRequired.Description)
            .MaximumLength(AuditLogErrors.EntityTypeMaxLength).WithMessage(AuditLogErrors.EntityTypeTooLong.Description);

        When(request => !string.IsNullOrWhiteSpace(request.Data), () =>
        {
            RuleFor(request => request.Data!)
                .MaximumLength(AuditLogErrors.DataMaxLength).WithMessage(AuditLogErrors.DataTooLong.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.IpAddress), () =>
        {
            RuleFor(request => request.IpAddress!)
                .MaximumLength(AuditLogErrors.IpAddressMaxLength).WithMessage(AuditLogErrors.IpAddressTooLong.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.UserAgent), () =>
        {
            RuleFor(request => request.UserAgent!)
                .MaximumLength(AuditLogErrors.UserAgentMaxLength).WithMessage(AuditLogErrors.UserAgentTooLong.Description);
        });
    }
}
