namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream source, int uploaderUserId, int? projectId, string fileExtension, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storedRelativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storedRelativePath, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string storedRelativePath);
}
