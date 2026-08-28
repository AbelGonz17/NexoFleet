using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Trips;

public sealed class TripFile : Entity
{
    internal TripFile(
        Guid id,
        Guid tripId,
        Guid companyId,
        string fileName,
        string storageKey,
        string contentType,
        long sizeInBytes,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc) : base(id)
    {
        TripId = tripId;
        CompanyId = companyId;
        FileName = fileName;
        StorageKey = storageKey;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    private TripFile() { }

    public Guid TripId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}
