namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<UploadDocumentsResponse> UploadDocumentsAsync(int requestingUserId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentSummaryDto>> GetMyDocumentsAsync(int requestingUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UploadProjectOptionDto>> GetAvailableUploadProjectsAsync(int requestingUserId, CancellationToken cancellationToken = default);
}
