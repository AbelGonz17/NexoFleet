using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Vehicles;

public sealed class VehicleDocument : Entity
{
    internal VehicleDocument(
        Guid id,
        Guid vehicleId,
        Guid companyId,
        VehicleDocumentType type,
        string fileName,
        string storageKey,
        string contentType,
        long sizeInBytes,
        DateOnly? expiresOn,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAtUtc) : base(id)
    {
        VehicleId = vehicleId;
        CompanyId = companyId;
        Type = type;
        FileName = fileName;
        StorageKey = storageKey;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        ExpiresOn = expiresOn;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    private VehicleDocument() { }

    public Guid VehicleId { get; private set; }
    public Guid CompanyId { get; private set; }
    public VehicleDocumentType Type { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}
