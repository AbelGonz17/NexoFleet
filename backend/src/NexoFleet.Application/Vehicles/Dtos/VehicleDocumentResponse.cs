using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Dtos;

public sealed record VehicleDocumentResponse(
    Guid Id,
    Guid VehicleId,
    Guid CompanyId,
    string Type,
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes,
    DateOnly? ExpiresOn,
    Guid UploadedByUserId,
    DateTimeOffset UploadedAtUtc)
{
    public static VehicleDocumentResponse FromDomain(VehicleDocument document) =>
        new(
            document.Id,
            document.VehicleId,
            document.CompanyId,
            document.Type.ToString(),
            document.FileName,
            document.StorageKey,
            document.ContentType,
            document.SizeInBytes,
            document.ExpiresOn,
            document.UploadedByUserId,
            document.UploadedAtUtc);
}
