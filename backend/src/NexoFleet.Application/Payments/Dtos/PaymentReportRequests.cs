using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Payments.Dtos;

public sealed record CreatePaymentReportRequest(
    Guid PaymentPeriodId,
    Guid EmployeeId,
    decimal BaseAmount,
    string Currency);

public sealed record UpdatePaymentReportBaseAmountRequest(
    decimal BaseAmount,
    string Currency);

public sealed record AddPaymentItemRequest(
    PaymentItemEffect Effect,
    string Description,
    decimal Amount,
    Guid? TripId = null);

public sealed record UpdatePaymentItemRequest(
    PaymentItemEffect Effect,
    string Description,
    decimal Amount);

public sealed record AddPaymentCommentRequest(string Text);

public sealed record AddPaymentReportFileRequest(
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes);

public sealed record VoidPaymentReportRequest(string Reason);
