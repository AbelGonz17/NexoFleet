using NexoFleet.Domain.Common;
using NexoFleet.Domain.Payments.Events;

namespace NexoFleet.Domain.Payments;

public sealed class PaymentPeriod : AggregateRoot
{
    private PaymentPeriod(Guid id, Guid companyId, string code, DateOnly startsOn, DateOnly endsOn, DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        Code = code;
        StartsOn = startsOn;
        EndsOn = endsOn;
        Status = PaymentPeriodStatus.Open;
        CreatedAtUtc = createdAtUtc;
    }

    private PaymentPeriod() { }

    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public PaymentPeriodStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<PaymentPeriod> Create(Guid id, Guid companyId, string code, DateOnly startsOn, DateOnly endsOn, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) return Result<PaymentPeriod>.Failure(PaymentErrors.InvalidId);
        if (companyId == Guid.Empty) return Result<PaymentPeriod>.Failure(PaymentErrors.InvalidCompanyId);
        if (string.IsNullOrWhiteSpace(code)) return Result<PaymentPeriod>.Failure(PaymentErrors.CodeRequired);
        if (code.Trim().Length > PaymentErrors.CodeMaxLength) return Result<PaymentPeriod>.Failure(PaymentErrors.CodeTooLong);
        if (endsOn < startsOn) return Result<PaymentPeriod>.Failure(PaymentErrors.InvalidPeriod);

        var period = new PaymentPeriod(id, companyId, code.Trim().ToUpperInvariant(), startsOn, endsOn, createdAtUtc);
        period.RaiseDomainEvent(new PaymentPeriodCreatedDomainEvent(id, companyId, createdAtUtc));
        return Result<PaymentPeriod>.Success(period);
    }

    public bool Contains(DateOnly date) => date >= StartsOn && date <= EndsOn;

    public Result Close(DateTimeOffset occurredAtUtc)
    {
        if (Status == PaymentPeriodStatus.Closed) return Result.Failure(PaymentErrors.AlreadyClosed);
        ChangeStatus(PaymentPeriodStatus.Closed, occurredAtUtc);
        return Result.Success();
    }

    public Result Reopen(DateTimeOffset occurredAtUtc)
    {
        if (Status == PaymentPeriodStatus.Open) return Result.Failure(PaymentErrors.AlreadyOpen);
        ChangeStatus(PaymentPeriodStatus.Open, occurredAtUtc);
        return Result.Success();
    }

    private void ChangeStatus(PaymentPeriodStatus status, DateTimeOffset occurredAtUtc)
    {
        var previous = Status;
        Status = status;
        UpdatedAtUtc = occurredAtUtc;
        RaiseDomainEvent(new PaymentPeriodStatusChangedDomainEvent(Id, CompanyId, previous, status, occurredAtUtc));
    }
}
