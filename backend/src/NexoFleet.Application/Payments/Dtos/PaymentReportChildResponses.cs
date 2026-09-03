using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Payments.Dtos;

public sealed record PaymentItemResponse(
    Guid Id,
    Guid PaymentReportId,
    Guid CompanyId,
    Guid? TripId,
    string Effect,
    string Description,
    decimal Amount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static PaymentItemResponse FromDomain(PaymentItem item) =>
        new(
            item.Id,
            item.PaymentReportId,
            item.CompanyId,
            item.TripId,
            item.Effect.ToString(),
            item.Description,
            item.Amount,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
}

public sealed record PaymentCommentResponse(
    Guid Id,
    Guid PaymentReportId,
    Guid CompanyId,
    Guid AuthorUserId,
    string Text,
    DateTimeOffset CreatedAtUtc)
{
    public static PaymentCommentResponse FromDomain(PaymentComment comment) =>
        new(
            comment.Id,
            comment.PaymentReportId,
            comment.CompanyId,
            comment.AuthorUserId,
            comment.Text,
            comment.CreatedAtUtc);
}

public sealed record PaymentReportFileResponse(
    Guid Id,
    Guid PaymentReportId,
    Guid CompanyId,
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes,
    Guid UploadedByUserId,
    DateTimeOffset UploadedAtUtc)
{
    public static PaymentReportFileResponse FromDomain(PaymentReportFile file) =>
        new(
            file.Id,
            file.PaymentReportId,
            file.CompanyId,
            file.FileName,
            file.StorageKey,
            file.ContentType,
            file.SizeInBytes,
            file.UploadedByUserId,
            file.UploadedAtUtc);
}
