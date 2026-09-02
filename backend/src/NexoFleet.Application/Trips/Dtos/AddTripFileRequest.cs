namespace NexoFleet.Application.Trips.Dtos;

public sealed record AddTripFileRequest(
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes);
