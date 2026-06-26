using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public LocalFileStorageService(IWebHostEnvironment environment, IOptions<DocumentOptions> options)
    {
        var configuredRoot = options.Value.StorageRoot?.Trim();
        var relativeRoot = string.IsNullOrWhiteSpace(configuredRoot) ? "AppData/uploads" : configuredRoot;
        _storageRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, relativeRoot));
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveAsync(Stream source, int uploaderUserId, int? projectId, string fileExtension, CancellationToken cancellationToken = default)
    {
        var normalizedExtension = fileExtension.StartsWith('.') ? fileExtension.ToLowerInvariant() : $".{fileExtension.ToLowerInvariant()}";
        var projectSegment = projectId?.ToString() ?? "personal";
        var relativePath = Path.Combine(
            uploaderUserId.ToString(),
            projectSegment,
            $"{Guid.NewGuid():N}{normalizedExtension}");

        var absolutePath = GetAbsolutePath(relativePath);
        var targetDirectory = Path.GetDirectoryName(absolutePath);

        if (!string.IsNullOrWhiteSpace(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        await using var targetStream = new FileStream(
            absolutePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await source.CopyToAsync(targetStream, cancellationToken);
        return relativePath.Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storedRelativePath, CancellationToken cancellationToken = default)
    {
        var stream = new FileStream(
            GetAbsolutePath(storedRelativePath),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string storedRelativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(storedRelativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    public string GetAbsolutePath(string storedRelativePath)
    {
        var combinedPath = Path.GetFullPath(Path.Combine(_storageRoot, storedRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!combinedPath.StartsWith(_storageRoot, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved storage path escaped the configured storage root.");
        }

        return combinedPath;
    }
}
