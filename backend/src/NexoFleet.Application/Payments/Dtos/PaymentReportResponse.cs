using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Payments.Dtos;

public sealed record PaymentReportResponse(
    Guid Id,
    Guid CompanyId,
    Guid PaymentPeriodId,
    Guid EmployeeId,
    decimal BaseAmount,
    string Currency,
    decimal Additions,
    decimal Deductions,
    decimal TotalAmount,
    string Status,
    DateTimeOffset? PublishedAtUtc,
    string? VoidedReason,
    IReadOnlyList<PaymentItemResponse> Items,
    IReadOnlyList<PaymentCommentResponse> Comments,
    IReadOnlyList<PaymentReportFileResponse> Files,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static PaymentReportResponse FromDomain(PaymentReport report) =>
        new(
            report.Id,
            report.CompanyId,
            report.PaymentPeriodId,
            report.EmployeeId,
            report.BaseAmount,
            report.Currency,
            report.Additions,
            report.Deductions,
            report.TotalAmount,
            report.Status.ToString(),
            report.PublishedAtUtc,
            report.VoidedReason,
            report.Items.OrderBy(i => i.CreatedAtUtc).Select(PaymentItemResponse.FromDomain).ToArray(),
            report.Comments.OrderBy(c => c.CreatedAtUtc).Select(PaymentCommentResponse.FromDomain).ToArray(),
            report.Files.OrderBy(f => f.UploadedAtUtc).Select(PaymentReportFileResponse.FromDomain).ToArray(),
            report.CreatedAtUtc,
            report.UpdatedAtUtc);
}
