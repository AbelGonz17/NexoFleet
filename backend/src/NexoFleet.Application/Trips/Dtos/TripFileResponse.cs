using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record TripFileResponse(
    Guid Id,
    Guid TripId,
    Guid CompanyId,
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes,
    Guid UploadedByUserId,
    DateTimeOffset UploadedAtUtc)
{
    public static TripFileResponse FromDomain(TripFile file) =>
        new(
            file.Id,
            file.TripId,
            file.CompanyId,
            file.FileName,
            file.StorageKey,
            file.ContentType,
            file.SizeInBytes,
            file.UploadedByUserId,
            file.UploadedAtUtc);
}
