using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments;

public sealed class PaymentItem : Entity
{
    internal PaymentItem(
        Guid id,
        Guid paymentReportId,
        Guid companyId,
        Guid? tripId,
        PaymentItemEffect effect,
        string description,
        decimal amount,
        DateTimeOffset createdAtUtc) : base(id)
    {
        PaymentReportId = paymentReportId;
        CompanyId = companyId;
        TripId = tripId;
        Effect = effect;
        Description = description;
        Amount = amount;
        CreatedAtUtc = createdAtUtc;
    }

    private PaymentItem() { }

    public Guid PaymentReportId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? TripId { get; private set; }
    public PaymentItemEffect Effect { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    internal void Update(PaymentItemEffect effect, string description, decimal amount, DateTimeOffset updatedAtUtc)
    {
        Effect = effect;
        Description = description;
        Amount = amount;
        UpdatedAtUtc = updatedAtUtc;
    }
}
