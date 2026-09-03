using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Payments.Dtos;

public sealed record PaymentPeriodResponse(
    Guid Id,
    Guid CompanyId,
    string Code,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static PaymentPeriodResponse FromDomain(PaymentPeriod period) =>
        new(
            period.Id,
            period.CompanyId,
            period.Code,
            period.StartsOn,
            period.EndsOn,
            period.Status.ToString(),
            period.CreatedAtUtc,
            period.UpdatedAtUtc);
}

public sealed record CreatePaymentPeriodRequest(
    string Code,
    DateOnly StartsOn,
    DateOnly EndsOn);
