using FluentValidation;
using NexoFleet.Application.Notifications.Dtos;
using NexoFleet.Domain.Notifications;

namespace NexoFleet.Application.Notifications.Validators;

public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(request => request.RecipientUserId)
            .NotEmpty().WithMessage(NotificationErrors.InvalidUserId.Description);

        RuleFor(request => request.Type)
            .IsInEnum().WithMessage(NotificationErrors.InvalidType.Description);

        RuleFor(request => request.Title)
            .NotEmpty().WithMessage(NotificationErrors.TitleRequired.Description)
            .MaximumLength(NotificationErrors.TitleMaxLength).WithMessage(NotificationErrors.TitleTooLong.Description);

        RuleFor(request => request.Message)
            .NotEmpty().WithMessage(NotificationErrors.MessageRequired.Description)
            .MaximumLength(NotificationErrors.MessageMaxLength).WithMessage(NotificationErrors.MessageTooLong.Description);

        When(request => request.RelatedEntityId.HasValue, () =>
        {
            RuleFor(request => request.RelatedEntityType)
                .NotEmpty().WithMessage(NotificationErrors.RelatedEntityTypeRequired.Description)
                .MaximumLength(NotificationErrors.RelatedEntityTypeMaxLength).WithMessage(NotificationErrors.RelatedEntityTypeTooLong.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.RelatedEntityType), () =>
        {
            RuleFor(request => request.RelatedEntityId)
                .NotNull().WithMessage(NotificationErrors.RelatedEntityIdRequired.Description);
        });
    }
}
