namespace ContosoDashboard.Services;

public interface IDocumentSafetyService
{
    Task<DocumentSafetyVerificationResult> VerifyAsync(DocumentUploadFile file, CancellationToken cancellationToken = default);
}

public class DocumentSafetyVerificationResult
{
    public bool IsSafe { get; init; }
    public string Message { get; init; } = string.Empty;
    public string NormalizedExtension { get; init; } = string.Empty;
    public string NormalizedContentType { get; init; } = string.Empty;
}
