using Microsoft.Extensions.Configuration;
using NexoFleet.Application.Abstractions.Storage;

namespace NexoFleet.Infrastructure.Storage;

internal sealed class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:LocalPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "uploads");

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyy/MM", System.Globalization.CultureInfo.InvariantCulture), uniqueFileName).Replace('\\', '/');
        var fullPath = Path.Combine(_basePath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return relativePath;
    }

    public Task<Stream?> DownloadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storageKey);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
