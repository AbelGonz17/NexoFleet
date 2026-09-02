using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles.Dtos;

public sealed record AddVehicleDocumentRequest(
    VehicleDocumentType Type,
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes,
    DateOnly? ExpiresOn);
