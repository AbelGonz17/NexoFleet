using FluentValidation;
using NexoFleet.Application.RouteSchedules.Dtos;
using NexoFleet.Domain.RouteSchedules;

namespace NexoFleet.Application.RouteSchedules.Validators;

public sealed class ConfigureRouteScheduleRecurrenceRequestValidator : AbstractValidator<ConfigureRouteScheduleRecurrenceRequest>
{
    public ConfigureRouteScheduleRecurrenceRequestValidator()
    {
        RuleFor(request => request.Shift)
            .IsInEnum().WithMessage(RouteScheduleErrors.InvalidShift.Description);

        RuleFor(request => request.Days)
            .NotEmpty().WithMessage(RouteScheduleErrors.DaysRequired.Description);

        When(request => request.Days is not null, () =>
        {
            RuleForEach(request => request.Days)
                .IsInEnum().WithMessage(RouteScheduleErrors.InvalidDayOfWeek.Description);
        });

        When(request => request.EndTime.HasValue, () =>
        {
            RuleFor(request => request.EndTime)
                .NotEqual(request => request.StartTime).WithMessage(RouteScheduleErrors.EndTimeEqualsStartTime.Description);
        });

        When(request => request.EffectiveUntil.HasValue, () =>
        {
            RuleFor(request => request.EffectiveUntil!.Value)
                .GreaterThanOrEqualTo(request => request.EffectiveFrom).WithMessage(RouteScheduleErrors.InvalidEffectivePeriod.Description);
        });

        When(request => request.DefaultAmount.HasValue, () =>
        {
            RuleFor(request => request.DefaultAmount!.Value)
                .GreaterThanOrEqualTo(0).WithMessage(RouteScheduleErrors.InvalidDefaultAmount.Description);

            RuleFor(request => request.DefaultCurrency)
                .NotEmpty().WithMessage(RouteScheduleErrors.DefaultCurrencyRequired.Description)
                .Length(RouteScheduleErrors.CurrencyLength).WithMessage(RouteScheduleErrors.DefaultCurrencyInvalid.Description);
        });

        When(request => !string.IsNullOrWhiteSpace(request.DefaultCurrency), () =>
        {
            RuleFor(request => request.DefaultAmount)
                .NotNull().WithMessage(RouteScheduleErrors.DefaultAmountRequired.Description);
        });
    }
}
