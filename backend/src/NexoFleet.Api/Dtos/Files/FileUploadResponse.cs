namespace NexoFleet.Api.Dtos.Files;

public sealed record FileUploadResponse(
    string FileName,
    string StorageKey,
    string ContentType,
    long SizeInBytes);
