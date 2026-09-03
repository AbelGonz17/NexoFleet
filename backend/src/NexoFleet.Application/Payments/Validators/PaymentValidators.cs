using FluentValidation;
using NexoFleet.Application.Payments.Dtos;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Payments.Validators;

public sealed class CreatePaymentPeriodRequestValidator : AbstractValidator<CreatePaymentPeriodRequest>
{
    public CreatePaymentPeriodRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty().WithMessage(PaymentErrors.CodeRequired.Description)
            .MaximumLength(PaymentErrors.CodeMaxLength).WithMessage(PaymentErrors.CodeTooLong.Description);

        RuleFor(request => request.EndsOn)
            .GreaterThanOrEqualTo(request => request.StartsOn).WithMessage(PaymentErrors.InvalidPeriod.Description);
    }
}

public sealed class CreatePaymentReportRequestValidator : AbstractValidator<CreatePaymentReportRequest>
{
    public CreatePaymentReportRequestValidator()
    {
        RuleFor(request => request.PaymentPeriodId)
            .NotEmpty().WithMessage(PaymentErrors.InvalidPeriodId.Description);

        RuleFor(request => request.EmployeeId)
            .NotEmpty().WithMessage(PaymentErrors.InvalidEmployeeId.Description);

        RuleFor(request => request.BaseAmount)
            .GreaterThanOrEqualTo(0).WithMessage(PaymentErrors.InvalidAmount.Description);

        RuleFor(request => request.Currency)
            .NotEmpty().WithMessage(PaymentErrors.CurrencyRequired.Description)
            .Length(PaymentErrors.CurrencyLength).WithMessage(PaymentErrors.CurrencyInvalid.Description);
    }
}

public sealed class UpdatePaymentReportBaseAmountRequestValidator : AbstractValidator<UpdatePaymentReportBaseAmountRequest>
{
    public UpdatePaymentReportBaseAmountRequestValidator()
    {
        RuleFor(request => request.BaseAmount)
            .GreaterThanOrEqualTo(0).WithMessage(PaymentErrors.InvalidAmount.Description);

        RuleFor(request => request.Currency)
            .NotEmpty().WithMessage(PaymentErrors.CurrencyRequired.Description)
            .Length(PaymentErrors.CurrencyLength).WithMessage(PaymentErrors.CurrencyInvalid.Description);
    }
}

public sealed class AddPaymentItemRequestValidator : AbstractValidator<AddPaymentItemRequest>
{
    public AddPaymentItemRequestValidator()
    {
        RuleFor(request => request.Effect)
            .IsInEnum().WithMessage(PaymentErrors.InvalidEffect.Description);

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage(PaymentErrors.DescriptionRequired.Description)
            .MaximumLength(PaymentErrors.DescriptionMaxLength).WithMessage(PaymentErrors.DescriptionTooLong.Description);

        RuleFor(request => request.Amount)
            .GreaterThanOrEqualTo(0).WithMessage(PaymentErrors.InvalidAmount.Description);

        When(request => request.TripId.HasValue, () =>
        {
            RuleFor(request => request.TripId!.Value)
                .NotEmpty().WithMessage(PaymentErrors.InvalidTripId.Description);
        });
    }
}

public sealed class UpdatePaymentItemRequestValidator : AbstractValidator<UpdatePaymentItemRequest>
{
    public UpdatePaymentItemRequestValidator()
    {
        RuleFor(request => request.Effect)
            .IsInEnum().WithMessage(PaymentErrors.InvalidEffect.Description);

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage(PaymentErrors.DescriptionRequired.Description)
            .MaximumLength(PaymentErrors.DescriptionMaxLength).WithMessage(PaymentErrors.DescriptionTooLong.Description);

        RuleFor(request => request.Amount)
            .GreaterThanOrEqualTo(0).WithMessage(PaymentErrors.InvalidAmount.Description);
    }
}

public sealed class AddPaymentCommentRequestValidator : AbstractValidator<AddPaymentCommentRequest>
{
    public AddPaymentCommentRequestValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty().WithMessage(PaymentErrors.CommentRequired.Description)
            .MaximumLength(PaymentErrors.CommentMaxLength).WithMessage(PaymentErrors.CommentTooLong.Description);
    }
}

public sealed class AddPaymentReportFileRequestValidator : AbstractValidator<AddPaymentReportFileRequest>
{
    public AddPaymentReportFileRequestValidator()
    {
        RuleFor(request => request.FileName)
            .NotEmpty().WithMessage(PaymentErrors.FileNameRequired.Description)
            .MaximumLength(PaymentErrors.FileNameMaxLength).WithMessage(PaymentErrors.FileMetadataTooLong.Description);

        RuleFor(request => request.StorageKey)
            .NotEmpty().WithMessage(PaymentErrors.StorageKeyRequired.Description)
            .MaximumLength(PaymentErrors.StorageKeyMaxLength).WithMessage(PaymentErrors.FileMetadataTooLong.Description);

        RuleFor(request => request.ContentType)
            .NotEmpty().WithMessage(PaymentErrors.ContentTypeRequired.Description)
            .MaximumLength(PaymentErrors.ContentTypeMaxLength).WithMessage(PaymentErrors.FileMetadataTooLong.Description);

        RuleFor(request => request.SizeInBytes)
            .GreaterThan(0).WithMessage(PaymentErrors.InvalidFileSize.Description);
    }
}

public sealed class VoidPaymentReportRequestValidator : AbstractValidator<VoidPaymentReportRequest>
{
    public VoidPaymentReportRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty().WithMessage(PaymentErrors.VoidReasonRequired.Description)
            .MaximumLength(PaymentErrors.ReasonMaxLength).WithMessage(PaymentErrors.ReasonTooLong.Description);
    }
}
