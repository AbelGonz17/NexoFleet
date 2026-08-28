using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments;

public sealed class PaymentComment : Entity
{
    internal PaymentComment(Guid id, Guid paymentReportId, Guid companyId, Guid authorUserId, string text, DateTimeOffset createdAtUtc) : base(id)
    {
        PaymentReportId = paymentReportId;
        CompanyId = companyId;
        AuthorUserId = authorUserId;
        Text = text;
        CreatedAtUtc = createdAtUtc;
    }

    private PaymentComment() { }

    public Guid PaymentReportId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
