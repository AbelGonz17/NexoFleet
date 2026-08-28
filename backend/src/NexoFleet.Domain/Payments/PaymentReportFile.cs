using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Payments;

public sealed class PaymentReportFile : Entity
{
    internal PaymentReportFile(
        Guid id,
        Guid paymentReportId,
        Guid companyId,
        string fileName,
        string storageKey,
        string contentType,
        long sizeInBytes,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc) : base(id)
    {
        PaymentReportId = paymentReportId;
        CompanyId = companyId;
        FileName = fileName;
        StorageKey = storageKey;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    private PaymentReportFile() { }

    public Guid PaymentReportId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}
